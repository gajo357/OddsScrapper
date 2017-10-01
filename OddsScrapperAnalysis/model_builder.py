# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import os
import sqlite3
from datetime import datetime
import pandas as pd
from sklearn.metrics import classification_report, confusion_matrix, roc_curve
from sklearn.externals import joblib
from sklearn.model_selection import cross_val_score, train_test_split, KFold, GridSearchCV, LeaveOneOut
from sklearn.pipeline import make_pipeline
from sklearn.naive_bayes import GaussianNB
from sklearn.svm import LinearSVC
from sklearn.linear_model import SGDClassifier, LogisticRegression
from sklearn.tree import ExtraTreeClassifier
from sklearn.neighbors import KNeighborsClassifier
from sklearn.neural_network import MLPClassifier
from sklearn.preprocessing import StandardScaler, OneHotEncoder
from sklearn.ensemble import GradientBoostingClassifier

features = ['SportIndex', 'CountryIndex', 'LeagueIndex', 'Bet', 'HomeTeamIndex', 'AwayTeamIndex', 'HomeOdd', 'DrawOdd', 'AwayOdd', 'IsPlayoffs', 'IsCup', 'IsWomen', 'Year', 'Month', 'Day'] # 
label = 'Winner'

sports = [(1, 'american-football'), (2, 'volleyball'), (3, 'rugby-union'), (4, 'rugby-league'), (5, 'hockey'), (6, 'handball'), (7, 'basketball'), (8, 'baseball'), (9, 'soccer'), (10, 'water-polo')]

def populate_index_column(cur):
    command = "SELECT Id from Countries;"
    all_ids = cur.execute(command).fetchall()
    for i, (country_id,) in enumerate(all_ids):
        command = "UPDATE Countries SET [Index]=? WHERE Id=?"
        cur.execute(command, (i, country_id))

    leagues = {}
    command = "select SportId,CountryId,Id from Leagues;"
    all_leagues = cur.execute(command).fetchall()
    for (sport_id, country_id, league_id) in all_leagues:
        i = 0
        if (sport_id, country_id) not in leagues:
            leagues[(sport_id, country_id)] = 0
        else:
            leagues[(sport_id, country_id)] += 1
            i = leagues[(sport_id, country_id)]

        command = "UPDATE Leagues SET [Index]=? WHERE Id=?"
        cur.execute(command, (i, league_id))
    
    teams = {}
    command = "SELECT LeagueId,Id from Teams;"
    all_teams = cur.execute(command).fetchall()
    for (league_id, team_id) in all_teams:
        i = 0
        if league_id not in teams:
            teams[league_id] = 0
        else:
            teams[league_id] += 1
            i = teams[league_id]

        command = "UPDATE Teams SET [Index]=? WHERE Id=?"
        cur.execute(command, (i, team_id))
    
    print('finished')

def create_columns(row, cur):
    league_id = row['LeagueId']
    home_team_id = row['HomeTeamId']
    away_team_id = row['AwayTeamId']

    command = "SELECT SportId,CountryId,IsWomen,IsCup,[Index] FROM Leagues WHERE Id=?;"
    sport_id, country_id, is_women, is_cup,league_index = cur.execute(command, (league_id,)).fetchone()

    command = "SELECT [Index] FROM Teams WHERE Id=?;"
    (home_index,) = cur.execute(command, (home_team_id,)).fetchone()
    (away_index,) = cur.execute(command, (away_team_id,)).fetchone()

    command = "SELECT [Index] FROM Countries WHERE Id=?;"
    (country_index,) = cur.execute(command, (country_id,)).fetchone()

    command = "SELECT [Index] FROM Sports WHERE Id=?;"
    (sport_index,) = cur.execute(command, (sport_id,)).fetchone()
    
    dt = datetime.strptime(row['Date'], "%m/%d/%Y %I:%M:%S %p")

    return (sport_index, country_index, league_index, away_index, home_index, 
            is_women, is_cup, dt.year, dt.month, dt.day)
        
def read_games_from_db(file_name):
    conn = sqlite3.connect(file_name)
    with conn:
        cur = conn.cursor()

        df = pd.read_sql_query("select * from Games;", conn)
        (df['SportIndex'], df['CountryIndex'], df['LeagueIndex'], df['AwayTeamIndex'], df['HomeTeamIndex'], 
        df['IsWomen'], df['IsCup'], df['Year'], df['Month'], df['Day']) = zip(*df.apply(lambda row: create_columns(row, cur), axis=1))

        cur.close()
    conn.close()

    return df

def load_data(file_name):
    useful = list(features)
    useful.append(label)
    useful.append('IsOvertime')

    data = read_games_from_db(file_name)
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
def find_best_clfs(data):
    X = data.as_matrix(features)
    y = data[label].values

    print('LinearSVC')
    scaler = StandardScaler()
    reg = LinearSVC(dual=False)
    pipe = make_pipeline(scaler, reg)
    parameters = {"linearsvc__C":[0.1, 1, 5, 10]}
    clf = GridSearchCV(pipe, parameters, n_jobs= 2)
    clf.fit(X, y)
    model = clf.best_estimator_
    print(model)

    print()
    print('ETC')
    scaler = StandardScaler()
    reg = ExtraTreeClassifier()
    pipe = make_pipeline(scaler, reg)
    parameters = {"extratreeclassifier__min_samples_leaf":[50, 70, 100, 150] }
    clf = GridSearchCV(pipe, parameters, n_jobs= 2)
    clf.fit(X, y)
    model = clf.best_estimator_
    print(model)

    print()
    print('KN')
    scaler = StandardScaler()
    reg = KNeighborsClassifier()
    pipe = make_pipeline(scaler, reg)
    parameters = {"kneighborsclassifier__n_neighbors":[2, 10, 30, 50], "kneighborsclassifier__leaf_size":[1, 5, 30], "kneighborsclassifier__weights":['distance', 'uniform'] }
    clf = GridSearchCV(pipe, parameters, n_jobs= 2)
    clf.fit(X, y)
    model = clf.best_estimator_
    print(model)

def train_different_clf(data):
    X = data.as_matrix(features)
    y = data[label].values

    ### split the data
    features_train, features_test, labels_train, labels_test = train_test_split(X, y, test_size=0.2, random_state=42)

    pred = pd.Series(labels_test)
    result = pd.DataFrame(pred, columns=['Winner'])
    result['Bet'] = pd.Series(features_test[:, 3]).astype(int)
    result['SportId'] = pd.Series(features_test[:, 0]).astype(int)
    result['LeagueId'] = pd.Series(features_test[:, 2]).astype(int)
    result['HomeOdd'] = pd.Series(features_test[:, 6])
    result['DrawOdd'] = pd.Series(features_test[:, 7])
    result['AwayOdd'] = pd.Series(features_test[:, 8])

    # print('Real result')
    # print(classification_report(labels_test, features_test[:, 3]))

    encoder = OneHotEncoder(categorical_features=[0, 1, 2, 4, 5, 12, 13, 14])
    encoder.fit(X)
    X = encoder.transform(X)
    features_train = encoder.transform(features_train)
    features_test = encoder.transform(features_test)

    scaler = StandardScaler(with_mean=False)
    scaler.fit(features_train)
    X = scaler.transform(X)
    features_train = scaler.transform(features_train)
    features_test = scaler.transform(features_test)
    
    # print('NB')
    # model = SGDClassifier(loss="log", penalty="l1")
    # pred, proba = validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    # result['NB'] = pred
    # result['NB_proba'] = proba
    
    print('LinearSVC')
    model = LinearSVC(dual=False, C=0.1)
    pred, proba = validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    result['SVC'] = pred
    result['SVC_proba'] = proba

    print('ETC')
    model = ExtraTreeClassifier(min_samples_leaf=50)
    pred, proba = validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    result['ETC'] = pred
    result['ETC_proba'] = proba
    
    print('K Neighbors')
    model = KNeighborsClassifier(n_neighbors=10, leaf_size=5, weights='distance')
    pred, proba = validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    result['KN'] = pred
    result['KN_proba'] = proba
    
    print('Neural')
    model = MLPClassifier(hidden_layer_sizes=(len(features)),max_iter=200, tol=1e-4)
    pred, proba = validate_model(model, X, y, features_train, labels_train, features_test, labels_test)
    result['MLP'] = pred
    result['MLP_proba'] = proba

    result.to_csv('test_predictions.csv', index=False)

def train_model(data, reg, save_model):
    X = data.as_matrix(features)
    y = data[label].values
    
    features_train, features_test, labels_train, labels_test = train_test_split(X, y, test_size = 0.2, random_state = 0)
    
    scaler = StandardScaler(with_mean=True)
    scaler.fit(features_train)
    if save_model:
        joblib.dump(scaler, 'models/scaler.pkl')
    features_train = scaler.transform(features_train)
    features_test = scaler.transform(features_test)

    print('Scaled')
        
    reg.fit(features_train, labels_train)
    
    print('Trained')
    if save_model:
        joblib.dump(reg, 'models/model.pkl')
    
    prediction = reg.predict(features_test)
    print('Predicted')
    y_true = pd.Series(labels_test)
    y_pred = pd.Series(prediction)
    print(pd.crosstab(y_true, y_pred, rownames=['True'], colnames=['Predicted'], margins=True))
    print()
    print(confusion_matrix(labels_test, prediction))
    print()
    print(classification_report(labels_test, prediction))
    print()

def train_neural_network(data):
    save_model = True
    X = data.as_matrix(features)
    y = data[label].values

    # split for train and test
    features_train, features_test, labels_train, labels_test = train_test_split(X, y,
                                                                test_size=0.2, random_state=42)
    print('Ready to train')

    encoder = OneHotEncoder(categorical_features=[0, 1, 2, 4, 5, 12, 13, 14])
    encoder.fit(X)
    if save_model:
        joblib.dump(encoder, 'models/encoder_nn.pkl')
    features_train = encoder.transform(features_train)
    features_test = encoder.transform(features_test)
    print('Encoded')

    scaler = StandardScaler(with_mean=False)
    scaler.fit(features_train)
    if save_model:
        joblib.dump(scaler, 'models/scaler_nn.pkl')
    features_train = scaler.transform(features_train)
    features_test = scaler.transform(features_test)

    no_features = features_train.shape[1]
    print(no_features)

    clf = MLPClassifier(learning_rate='constant', solver='adam', activation='relu',
        alpha=1e-4, hidden_layer_sizes=(int(no_features*0.7),), )
    clf.fit(features_train, labels_train)
    if save_model:
        joblib.dump(clf, 'models/model_nn.pkl')

    predictions = clf.predict(features_test)
    print('Predictions done')

    y_true = pd.Series(labels_test)
    y_pred = pd.Series(predictions)
    print(pd.crosstab(y_true, y_pred, rownames=['True'], colnames=['Predicted'], margins=True))
    print()
    print(classification_report(labels_test, predictions))
    print()


def find_best_neural_network(data):
    save_model = True
    X = data.as_matrix(features)
    y = data[label].values

    print('Ready to train')

    encoder = OneHotEncoder(categorical_features=[0, 1, 2, 4, 5, 12, 13, 14])
    encoder.fit(X)
    if save_model:
        joblib.dump(encoder, 'models/encoder.pkl')
    X = encoder.transform(X)
    print('Encoded')

    scaler = StandardScaler(with_mean=False)
    scaler.fit(X)
    X = scaler.transform(X)

    no_features = X.shape[1]
    print(no_features)

    clf = MLPClassifier(learning_rate='constant', solver='adam')

    scaler = StandardScaler(with_mean=False)
    pipe = make_pipeline(clf)
    parameters = {"mlpclassifier__activation":['relu', 'logistic'], "mlpclassifier__alpha":[1e-5, 1e-4,1e-3], "mlpclassifier__hidden_layer_sizes":[(no_features,), (int(no_features * 0.6),), (int(no_features * 0.7),)], }
    grid = GridSearchCV(pipe, parameters, n_jobs=2)
    grid.fit(X, y)
    clf = grid.best_estimator_
    print(clf)

    print('Trained')
    if save_model:
        joblib.dump(clf, 'models/model.pkl')
    
    prediction = clf.predict(X)
    print('Predicted')
    y_true = pd.Series(Y)
    y_pred = pd.Series(prediction)
    print(pd.crosstab(y_true, y_pred, rownames=['True'], colnames=['Predicted'], margins=True))
    print()
    print(classification_report(labels_test, prediction))
    print()
    

def train_gradient_boost(data):
    save_model = True
    X = data.as_matrix(features)
    y = data[label].values

    # split for train and test
    features_train, features_test, labels_train, labels_test = train_test_split(X, y,
                                                                test_size=0.2, random_state=42)
    # split the data for both classifier and regressor
    features_train, features_train_lr, labels_train, labels_train_lr = train_test_split(features_train,
        labels_train, test_size=0.5, random_state=42)
    
    pred = pd.Series(labels_test)
    result = pd.DataFrame(pred, columns=['Winner'])
    result['SportIndex'] = pd.Series(features_test[:, 0]).astype(int)
    result['Bet'] = pd.Series(features_test[:, 3]).astype(int)
    result['HomeOdd'] = pd.Series(features_test[:, 6])
    result['DrawOdd'] = pd.Series(features_test[:, 7])
    result['AwayOdd'] = pd.Series(features_test[:, 8])

    print('Ready to train')
    # create classifier, train it on training data
    # grd = GradientBoostingClassifier(n_estimators=1000)
    # grd.fit(features_train, labels_train)
    # if save_model:
    #     joblib.dump(grd, 'models/model_grd.pkl')
    grd = joblib.load('models/model_grd.pkl')
    print('Classifier fitted')
    # create encoder
    # encoder = OneHotEncoder(categorical_features=[0, 1, 2, 4, 5, 12, 13, 14])
    # # fit it on the output of classifier
    # encoder.fit(grd.apply(features_train)[:, :, 0])
    # if save_model:
    #     joblib.dump(encoder, 'models/encoder_grd.pkl')
    encoder = joblib.load('models/encoder_grd.pkl')
    print('encoder fitted')
    # # create LogisticRegression
    # grd_lm = LogisticRegression()
    # # fit on encoded output of classifier
    # grd_lm.fit(encoder.transform(grd.apply(features_train_lr)[:, :, 0]), labels_train_lr)
    # if save_model:
    #     joblib.dump(grd_lm, 'models/model_grd_lm.pkl')
    grd_lm = joblib.load('models/model_grd_lm.pkl')
    print('Regressor fitted')

    enc = encoder.transform(grd.apply(features_test)[:, :, 0])
    predictions = grd_lm.predict(enc)
    probabilities = grd_lm.predict_proba(enc)
    win_proba = []
    # interested only in probability of predicted result
    for i, proba in enumerate(probabilities):
        win_proba.append(proba[predictions[i]])
    print('Predictions done')

    pred = pd.Series(labels_test)
    result = pd.DataFrame(pred, columns=['Winner'])
    result['Prediction'] = predictions.tolist()
    result['Probability'] = win_proba
    result.to_csv('test_predictions_grd.csv', index=False)

    y_true = pd.Series(labels_test)
    y_pred = pd.Series(predictions)
    print(pd.crosstab(y_true, y_pred, rownames=['True'], colnames=['Predicted'], margins=True))
    print()
    print(classification_report(labels_test, predictions))
    print()

if __name__ == '__main__':
    # db_name = os.path.abspath(os.path.join(os.path.dirname(__file__),\
    #                         os.pardir, 'ArchiveData.db'))
    # db_data = load_data(db_name)

    db_data = pd.read_csv('archive.csv')

    #train_different_clf(db_data)
    # train_model(db_data, clf, False)
    #find_best_clfs(db_data)
    # train_neural_network(db_data)
    train_gradient_boost(db_data)
    print('Done')
