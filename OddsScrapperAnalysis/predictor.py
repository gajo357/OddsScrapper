# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import os
import pandas as pd
from sklearn.externals import joblib
#from model_builder import features

features = ['SportId', 'CountryId', 'LeagueId', 'Bet', 'HomeTeamId', 'AwayTeamId', 'HomeOdd', 'DrawOdd', 'AwayOdd', 'IsPlayoffs', 'IsCup', 'IsWomen'] # 

def calculate_kelly(probability, odd):
    return 1.0 * (probability * odd - 1.0) / (odd - 1.0)
    
def calculate_kelly_from_row(row):
    win_probability = row['Probability']
    bet = row['Prediction']
    odd = -1
    if bet == 0:
        odd = row['DrawOdd']
    elif bet == 1:
        odd = row['HomeOdd']
    elif bet == 2:
        odd = row['AwayOdd']
        
    return calculate_kelly(win_probability, odd)

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
    games_df['Kelly'] = games_df.apply(lambda row: calculate_kelly_from_row(row), axis=1)

    games_df.sort_values(by='Kelly', ascending=False, inplace=True)
    games_df.to_csv(games_to_bet_file, index=False)

def create_columns(row, leagues_info):
    league_id = row['LeagueId']
    bet = row['Bet']
    odd = None
    if bet == 0:
        odd = row['DrawOdd']
    elif bet == 1:
        odd = row['HomeOdd']
    elif bet == 2:
        odd = row['AwayOdd']

    if not odd or odd <= 1:
        return (0, 0)

    
    leagues = leagues_info[(leagues_info['LeagueId'] == league_id) & (leagues_info['Bet'] == bet) & (leagues_info['MinOdd'] <= odd) & (leagues_info['MaxOdd'] > odd)]
    if leagues.empty:
        return (0, 0)
    league = leagues.iloc[0]
    probability = league['Score']
    return (probability, calculate_kelly(probability, league['MinOdd']))

def predict_results_byleague(date_str, games_to_bet_file):
    file_name = 'AnalysedFiles/best_leagues.csv'
    leagues_info = pd.read_csv(file_name)

    games_df = pd.read_csv(games_to_bet_file, encoding="ISO-8859-1")
    games_df.dropna(axis=0, how='any', inplace=True)
    games_df = games_df[~games_df.isin(['NaN']).any(axis=1)]

    games_df['ProbaLeague'], games_df['Kelly'] = zip(*games_df.apply(lambda row: create_columns(row, leagues_info), axis=1))
        
    # interested only in probability of predicted result
    games_df = games_df[(games_df['ProbaLeague'] > 0) & (games_df['Kelly'] >= 0)]
    games_df.sort_values(by='Kelly', ascending=False, inplace=True)

    games_df = games_df[['Sport','Country','League','Participants','Bet','HomeOdd','DrawOdd','AwayOdd','ProbaLeague','Kelly','Probability','Prediction']]
    games_df.to_csv(games_to_bet_file.replace(date_str, '{0}_good'.format(date_str)), index=False)

if __name__ == '__main__':
    date_str = '23Oct2017'
    games_file = os.path.abspath(os.path.join(os.path.dirname(__file__),\
                            os.pardir, 'OddsScrapper', 'TommorowsGames', 'games_{}.csv'.format(date_str)))

    predict_results(games_file)
    predict_results_byleague(date_str, games_file)

    print('Done')