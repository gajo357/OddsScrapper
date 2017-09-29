# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import os
import pandas as pd
from sklearn.externals import joblib
#from model_builder import features

features = ['SportId', 'CountryId', 'LeagueId', 'Bet', 'HomeTeamId', 'AwayTeamId', 'HomeOdd', 'DrawOdd', 'AwayOdd', 'IsPlayoffs', 'IsCup', 'IsWomen'] # 

def calculate_kelly(row):
    win_probability = row['Probability']
    bet = row['Prediction']
    odd = -1
    if bet == 0:
        odd = row['DrawOdd']
    elif bet == 1:
        odd = row['HomeOdd']
    elif bet == 2:
        odd = row['AwayOdd']
        
    return 1.0 * (win_probability * odd - 1.0) / (odd - 1.0)

def predict_results(games_to_bet_file):
    games_df = pd.read_csv(games_to_bet_file, encoding="ISO-8859-1")
    
    games_df.dropna(axis=0, how='any', inplace=True)
    games_df = games_df[~games_df.isin(['NaN']).any(axis=1)]

    games = games_df.as_matrix(features)

    scaler = joblib.load('models/scaler.pkl')
    games = scaler.transform(games)
    reg = joblib.load('models/model.pkl')
    predictions = reg.predict(games)
    probabilities = reg.predict_proba(games)
    
    # interested only in probability of predicted result
    win_proba = []
    kelly = []
    for i, proba in enumerate(probabilities):
        win_proba.append(proba[predictions[i]])

    games_df['Prediction'] = predictions.tolist()
    games_df['Probability'] = win_proba
    games_df['Kelly'] = games_df.apply(lambda row: calculate_kelly(row), axis=1)

    games_df.sort_values(by='Kelly', ascending=False, inplace=True)
    games_df.to_csv(games_to_bet_file, index=False)

if __name__ == '__main__':
    date_str = '30Sep2017'
    games_file = os.path.abspath(os.path.join(os.path.dirname(__file__),\
                            os.pardir, 'OddsScrapper', 'TommorowsGames', 'games_{}.csv'.format(date_str)))

    predict_results(games_file)

    print('Done')
    
