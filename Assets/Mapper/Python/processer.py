from __future__ import division, print_function

import unity_client

import os
import socket

import threading
import time
import sys
import spacy
import pandas as pd

try_to_connect = True

client_name = "com.unity.scripting.python.clients.processer"


class TextProcessing:
    nlp = spacy.load('en_core_web_md')

    dataframe = pd.DataFrame(
        columns=['first_object', 'fObj_desc', 'second_object', 'sObj_desc', 'distance',
                 'rotation_side', 'rotation_up', 'bool_is_internal'])

    file_pos = pd.read_excel("Assets/Mapper/prepositions.xlsx")

    objects = []

    def add_obj_info(self, connection, noun_1, noun_2):
        connection['first_object'] = noun_2.lemma_
        info = [tempHolder for tempHolder in noun_2.children if
                tempHolder.dep_ == "amod" or tempHolder.dep_ == "compound"]
        desc_string = ""
        for str in info:
            desc_string = desc_string + str.lemma_ + ' '
        connection['fObj_desc'] = desc_string[:-1]

        connection['second_object'] = noun_1.lemma_
        info = [tempHolder for tempHolder in noun_1.children if
                tempHolder.dep_ == "amod" or tempHolder.dep_ == "compound"]
        desc_string = ""
        for str in info:
            desc_string = desc_string + str.lemma_ + ' '
        connection['sObj_desc'] = desc_string[:-1]

    def add_num_info(self, connection, words):
        connection['distance'] = -1
        connection['rotation_side'] = -1
        connection['rotation_up'] = -1
        connection['bool_is_internal'] = -1

        for word in words:
            if word in self.file_pos["word"].values:
                needed_row = self.file_pos.loc[self.file_pos['word'] == word]

                if needed_row['distance'].values[0] > -1:
                    connection['distance'] = needed_row['distance'].values[0]
                if needed_row['rotation_side'].values[0] > -1:
                    connection['rotation_side'] = needed_row['rotation_side'].values[0]
                if needed_row['rotation_up'].values[0] > -1:
                    connection['rotation_up'] = needed_row['rotation_up'].values[0]
                if needed_row['bool_is_internal'].values[0] > -1:
                    connection['bool_is_internal'] = needed_row['bool_is_internal'].values[0]

    def process_farFrom_nearTo(self, first_noun, elem):
        connection = {}

        word_fars = [tempHolder for tempHolder in elem.children if tempHolder.dep_ == "advmod" or tempHolder.dep_ == "prep"]

        for word_far in word_fars:
            preps_1 = [tempHolder for tempHolder in word_far.children if tempHolder.dep_ == "prep"]

            for prep_1 in preps_1:

                second_nouns = [tempHolder for tempHolder in prep_1.children if
                                tempHolder.pos_ == "NOUN" or tempHolder.pos_ == "PROPN"]

                for second_noun in second_nouns:
                    if second_noun.lemma_ in self.objects:
                        self.add_obj_info(connection, first_noun, second_noun)

                        words_to_process = [elem.lemma_, word_far.lemma_, second_noun.lemma_]
                        self.add_num_info(connection, words_to_process)

                        self.dataframe = self.dataframe.append(connection, ignore_index=True)

    def extract_by_prepositions(self, sent):

        first_nouns_chunks = [tempHolder for tempHolder in sent.noun_chunks if tempHolder.root.lemma_ in self.objects]
        for first_nouns_chunk in first_nouns_chunks:

            first_noun = first_nouns_chunk.root

            connection = {}

            arr_elToCheck = [first_noun]
            if first_noun.head.pos_ == "VERB" or first_noun.head.pos_ == "AUX":
                arr_elToCheck.append(first_noun.head)

            for elem in arr_elToCheck:

                self.process_farFrom_nearTo(first_noun, elem)

                preps_1 = [tempHolder for tempHolder in elem.children if tempHolder.dep_ == "prep"]

                for prep_1 in preps_1:

                    mid_nouns = [tempHolder for tempHolder in prep_1.children if
                                 tempHolder.pos_ == "NOUN" or tempHolder.pos_ == "PROPN"]

                    for mid_noun in mid_nouns:

                        if mid_noun.lemma_ in self.file_pos["word"].values:

                            preps_2 = [tempHolder for tempHolder in mid_noun.children if tempHolder.dep_ == "prep"]

                            for prep_2 in preps_2:

                                second_nouns = [tempHolder for tempHolder in prep_2.children if
                                                tempHolder.pos_ == "NOUN" or tempHolder.pos_ == "PROPN"]

                                for second_noun in second_nouns:
                                    if second_noun.lemma_ in self.objects:
                                        self.add_obj_info(connection, first_noun, second_noun)

                                        words_to_process = [elem.lemma_, prep_1.lemma_, mid_noun.lemma_, prep_2.lemma_]
                                        self.add_num_info(connection, words_to_process)

                                        self.dataframe = self.dataframe.append(connection, ignore_index=True)

                        else:
                            if mid_noun.lemma_ in self.objects:
                                self.add_obj_info(connection, first_noun, mid_noun)

                                words_to_process = [elem.lemma_, prep_1.lemma_, mid_noun.lemma_]
                                self.add_num_info(connection, words_to_process)

                                self.dataframe = self.dataframe.append(connection, ignore_index=True)

    def extract_objects_positions(self, obj, text):
        doc = self.nlp(text)
        self.objects = obj.split(' ')
        for sent in doc.sents:
            self.extract_by_prepositions(sent)
        self.dataframe.to_json("Assets/Mapper/result.json", orient='records')


class ProcesserClientService(unity_client.UnityClientService):

    def __init__(self):
        self._globals = dict()

    def exposed_client_name(self):
        return client_name

    def exposed_globals(self):
        return self._globals

    def exposed_try_spacy(self, objects, text):
        proc = TextProcessing()
        proc.extract_objects_positions(objects, text)

    def exposed_on_server_shutdown(self, invite_retry):
        global try_to_connect
        try_to_connect = invite_retry
        super(ProcesserClientService, self).exposed_on_server_shutdown(invite_retry)

if __name__ == '__main__':
    while try_to_connect:
        time.sleep(0.5)
        try:
            c = unity_client.connect(ProcesserClientService)
        except socket.error:
            print("failed to connect; try again later")
            continue

        print("connected")
        try:
            c.thread.join()
        except KeyboardInterrupt:
            c.close()
            c.thread.join()
        print("disconnected")