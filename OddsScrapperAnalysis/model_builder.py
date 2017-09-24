# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import os
import sqlite3
import pandas as pd
from sklearn.metrics import classification_report
from sklearn.externals import joblib
from sklearn.model_selection import cross_val_score, train_test_split, KFold
from sklearn.naive_bayes import GaussianNB
from sklearn.tree import ExtraTreeClassifier
from sklearn.neighbors import KNeighborsClassifier

features = ['SportId', 'CountryId', 'LeagueId', 'Bet', 'HomeTeamId', 'AwayTeamId', 'HomeOdd', 'DrawOdd', 'AwayOdd', 'IsPlayoffs', 'IsCup', 'IsWomen'] # 
label = 'Winner'

sports = [(1, 'american-football'), (2, 'volleyball'), (3, 'rugby-union'), (4, 'rugby-league'), (5, 'hockey'), (6, 'handball'), (7, 'basketball'), (8, 'baseball'), (9, 'soccer'), (10, 'water-polo')]

def create_columns(league_id, cur):
    command = "select SportId,CountryId,Name,IsWomen,IsCup from Leagues where Id = '{0}';".format(league_id)
    sport_id, country_id, name, is_women, is_cup = cur.execute(command).fetchone()
    
    return (sport_id, country_id, is_women, is_cup)
        
def read_games_from_db(db_name):
    conn = sqlite3.connect(db_name)
    cur = conn.cursor()

    df = pd.read_sql_query("select * from Games;", conn)
    df['SportId'], df['CountryId'], df['IsWomen'], df['IsCup'] = zip(*df.apply(lambda row: create_columns(row['LeagueId'], cur), axis=1))

    cur.close()
    conn.close()
    return df

def load_data(db_name):
    useful = list(features)
    useful.append(label)
    useful.append('IsOvertime')

    data = read_games_from_db(db_name)
    data = data[useful]
    data.dropna(axis=0, how='any', inplace=True)
    data = data[~data.isin(['NaN']).any(axis=1)]

    data.to_csv('archive.csv', index=False)
    return data

def validate_model(model, X, y, features_train, labels_train, features_test, labels_test):
    model.fit(features_train, labels_train)
    print(model.score(features_train, labels_train))
    print(model.score(features_test, labels_test))

    predictions = model.predict(features_test)
    win_proba = []
    probabilities = model.predict_proba(features_test)
    # interested only in probability of predicted result
    for i, proba in enumerate(probabilities):
        win_proba.append(proba[predictions[i]])

    print(classification_report(labels_test, predictions))

    shuffle = KFold(n_splits=5, shuffle=True, random_state=0)
    print(cross_val_score(model, X, y, cv = shuffle))
    return (predictions.tolist(), win_proba)

def train_different_clf(data):
    X = data.as_matrix(features)
    y = data[label].values
    ### split the data
    features_train, features_test, labels_train, labels_test = train_test_split(X, y, test_size = 0.2, random_state = 42)
    
    pred = pd.Series(labels_test)
    result = pd.DataFrame(pred, columns=['Winer'])
    result['Bet'] = pd.Series(features_test[:, 3]).astype(int)
    result['SportId'] = pd.Series(features_test[:, 0]).astype(int)
    result['LeagueId'] = pd.Series(features_test[:, 2]).astype(int)
    result['HomeOdd'] = pd.Series(features_test[:, 6])
    result['DrawOdd'] = pd.Series(features_test[:, 7])
    result['AwayOdd'] = pd.Series(features_test[:, 8])

    print('Real result')
    print(classification_report(labels_test, features_test[:, 3]))
    
    print('Gaussian NB')
    model = GaussianNB()
    pred, proba = validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    result['NB'] = pred
    result['NB_proba'] = proba

    print('ETC')
    model = ExtraTreeClassifier(min_samples_leaf=2)
    pred, proba = validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    result['ETC'] = pred
    result['ETC_proba'] = proba
    
    print('K Neighbors')
    model = KNeighborsClassifier(n_neighbors=10, leaf_size=5, weights='distance')
    pred, proba = validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    result['KN'] = pred
    result['KN_proba'] = proba

    result.to_csv('test_predictions.csv', index=False)

def train_model(data, reg, save_model):
    X = data.as_matrix(features)
    y = data[label].values
    
    features_train, features_test, labels_train, labels_test = train_test_split(X, y, test_size = 0.02, random_state = 0)
    
    reg.fit(features_train, labels_train) 
    
    if save_model:
        joblib.dump(reg, 'models/model.pkl')
    
    print(reg.score(features_train, labels_train))
    print(reg.score(features_test, labels_test))
    prediction = reg.predict(features_test)
    
    y_true = pd.Series(labels_test)
    y_pred = pd.Series(prediction)
    print(pd.crosstab(y_true, y_pred, rownames=['True'], colnames=['Predicted'], margins=True))
    print(classification_report(labels_test, prediction))
    print()


def analyse_single(df, column):
    #sub = df[df['BET'] == df[column]]
    sub = df
    if sub.shape[0] < 10:
        return
    
    print(column)
    print(classification_report(sub['WIN'].values, sub[column].values))

def analyse_model(df, column):
    analyse_single(df, column)
    proba = '{}_proba'.format(column)
    analyse_single(df[df[proba] >= 0.9], column)

def analyse_all():
    data = pd.read_csv('predictions.csv')
    for sport_id, sport in sports:        
        df = data[data['SportId'] == sport_id]

        print(sport)
        
        print()


        analyse_single(df, 'BET')

        analyse_model(df, 'NB')
        analyse_model(df, 'DTC')
        analyse_model(df, 'ETC')
        analyse_model(df, 'KN')

        sub = df[(df['BET'] == df['DTC']) & (df['BET'] == df['ETC']) & (df['BET'] == df['KN']) & (df['BET'] == df['NB'])]
        win = sub['WIN']

        print('ALL')
        print(classification_report(win.values, sub['BET'].values))

if __name__ == '__main__':
    db_name = os.path.abspath(os.path.join(os.path.dirname(__file__),\
                            os.pardir, 'ArchiveData.db'))
    db_data = load_data(db_name)

    db_data = pd.read_csv('archive.csv')
    #db_data = db_data[db_data['SportId'] == 9]
    #db_data = db_data[(db_data['HomeOdd'] >= 2) & (db_data['AwayOdd'] >= 2)]

    train_different_clf(db_data)
    #train_model(db_data, ExtraTreeClassifier(min_samples_leaf=2), True)

    #analyse_all()
