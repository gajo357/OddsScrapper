# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import os
import sqlite3
import pandas as pd
from sklearn.externals import joblib
from sklearn.metrics import confusion_matrix
from sklearn.model_selection import cross_val_score, train_test_split, KFold
from sklearn.naive_bayes import GaussianNB
from sklearn.tree import DecisionTreeClassifier, ExtraTreeClassifier
from sklearn.neighbors import KNeighborsClassifier

features = ['HomeTeamId', 'AwayTeamId', 'LeagueId', 'WinningOdd', 'Bet'] #, 'Season' 
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
    shuffle = KFold(n_splits=5, shuffle=True, random_state=0)
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
    
    features_train, features_test, labels_train, labels_test = train_test_split(X, y, test_size = 0.2, random_state = 0)
    
    reg = model_creator()
    reg.fit(features_train, labels_train) 
    
    if save_model:
        joblib.dump(reg, 'models/model.pkl')
    
    print(reg.score(features_train, labels_train))
    print(reg.score(features_test, labels_test))
    prediction = reg.predict(features_test)
    
    y_true = pd.Series(labels_test)
    y_pred = pd.Series(prediction)
    print(pd.crosstab(y_true, y_pred, rownames=['True'], colnames=['Predicted'], margins=True))

    print(confusion_matrix(labels_test, prediction))
    print()


def data_stats(data):
    data_count = data.shape[0]
    print('data count')
    print(data_count)

    home_winners = data[data['Bet'] == 1][data['Winner'] == 1]['Winner'].count()
    home_bets = data[data['Bet'] == 1]['Bet'].count()
    print('home bet percentage')
    print(home_bets * 1.0 / data_count)
    print('home win percentage')
    print(home_winners * 1.0 / home_bets)

    away_winners = data[data['Bet'] == 2][data['Winner'] == 2]['Winner'].count()
    away_bets = data[data['Bet'] == 2]['Bet'].count()
    print('away bet percentage')
    print(away_bets * 1.0 / data_count)
    print('away win percentage')
    print(away_winners * 1.0 / away_bets)

    win_count = data[data['Bet'] == data['Winner']]['Bet'].count()
    print('win percentage')
    print(win_count * 1.0 / data_count)

if __name__ == '__main__':
    db_name = os.path.abspath(os.path.join(os.path.dirname(__file__),\
                            os.pardir, 'OddsWebsite', 'ArchiveData.db'))

    db_data = load_data(db_name)
    #data_stats(db_data)
    #train_different_clf(db_data)
    train_model(db_data, ExtraTreeClassifier, True)
