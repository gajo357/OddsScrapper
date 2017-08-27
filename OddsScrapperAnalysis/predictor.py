# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import os
import sqlite3
import pandas as pd
from sklearn.externals import joblib


if __name__ == '__main__':
    games_to_bet = os.path.abspath(os.path.join(os.path.dirname(__file__),\
                            os.pardir, 'OddsScrapper', 'GamesToBet', 'GamesToBet_27Aug2017_all_1'))