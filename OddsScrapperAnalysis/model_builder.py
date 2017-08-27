# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import os
import sqlite3
import pandas as pd
from sklearn.externals import joblib
from sklearn.cross_validation import KFold
from sklearn.model_selection import cross_val_score, train_test_split
from sklearn.naive_bayes import GaussianNB
from sklearn.tree import DecisionTreeClassifier, ExtraTreeClassifier
from sklearn.neighbors import KNeighborsClassifier

features = ['HomeTeamId', 'AwayTeamId', 'LeagueId', 'Season', 'WinningOdd', 'Bet']
label = 'Winner'

def read_games_from_db(db_name):
    conn = sqlite3.connect(db_name)

    df = pd.read_sql_query("select * from Games;", conn)
    
    conn.close()
    return df

def load_data(db_name):
    useful = list(features)
    useful.append(label)

    data = read_games_from_db(db_name)
    data = data[useful]
    data.dropna(axis=0, how='any', inplace=True)
    return data

def validate_model(model, X, y, features_train, labels_train, features_test, labels_test):
    shuffle = KFold(len(X), n_folds=5, shuffle=True, random_state=0)
    model.fit(features_train, labels_train)
    print(model.score(features_train, labels_train))
    print(model.score(features_test, labels_test))
    print(cross_val_score(model, X, y, cv = shuffle))

def train_different_clf(data):
    X = data.as_matrix(features)
    y = data[label].values
    ### split the data
    features_train, features_test, labels_train, labels_test = train_test_split(X, y, test_size = 0.2, random_state = 42)
    
    
    print('NB')
    model = GaussianNB()
    validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    
    print('DTC')
    model = DecisionTreeClassifier()
    validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    
    print('ETC')
    model = ExtraTreeClassifier()
    validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    
    print('K Neighbors')
    model = KNeighborsClassifier()
    validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    

def train_model(data, model_creator, save_model):    
    X = data.as_matrix(features)
    y = data[label].values
    
    features_train, features_test, labels_train, labels_test = train_test_split(X, y, test_size = 0.4, random_state = 0)
    
    reg = model_creator()
    reg.fit(features_train, labels_train) 
    
    if save_model:
        joblib.dump(reg, 'models/model.pkl')
    
    print(reg.score(features_train, labels_train))
    print(reg.score(features_test, labels_test))
    print()

if __name__ == '__main__':
    db_name = os.path.abspath(os.path.join(os.path.dirname(__file__),\
                            os.pardir, 'OddsWebsite', 'ArchiveData.db'))

    db_data = load_data(db_name)
    train_different_clf(db_data)
    #train_model(data, ExtraTreeClassifier, True)
