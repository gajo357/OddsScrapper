# -*- coding: utf-8 -*-
"""A module that tests trained sklear models for predicting data.
"""

import os
import sqlite3
import pandas as pd
from sklearn.externals import joblib
from model_builder import features

sports = [(1, 'american-football'), (2, 'volleyball'), (3, 'rugby-union'), (4, 'rugby-league'), (5, 'hockey'), (6, 'handball'), (7, 'basketball'), (8, 'baseball'), (9, 'soccer'), (10, 'water-polo')]


def get_participant(participants, first):
    pars = participants.replace("&nbsp;", '').replace("\'", '').split(' - ', 1)
    if first:
        return pars[0]
    return pars[1]

def create_columns(row, cur):
    league_id = 0
    country_id = 0
    home_team_id = 0
    away_team_id = 0
    duplicate = False

    row_odd = float(row['WinningOdd'])
    row_bet = int(row['Bet'])

    home_team = get_participant(row['Participants'], True)
    away_team = get_participant(row['Participants'], False)

    command = "select Id from Sports where Name = '{0}';".format(row['Sport'])
    sport_id = cur.execute(command).fetchone()[0]    
    command = "select Id from Countries where Name = '{0}';".format(row['Country'])
    country_id = cur.execute(command).fetchone()
    if country_id:
        country_id = country_id[0]
    
        command = "select Id from Leagues where Name = '{0}' AND SportId = '{1}' AND CountryId = '{2}';".format(row['League'], sport_id, country_id)
        league_id = cur.execute(command).fetchone()
        if league_id:
            league_id = league_id[0]

            command = "select Id from Teams where Name = '{0}' AND LeagueId = '{1}';".format(home_team, league_id)
            home_team_id = cur.execute(command).fetchone()
            command = "select Id from Teams where Name = '{0}' AND LeagueId = '{1}';".format(away_team, league_id)
            away_team_id = cur.execute(command).fetchone()

            if not home_team_id:
                home_team_id = 0
            else:
                home_team_id = home_team_id[0]
            if not away_team_id:
                away_team_id = 0
            else:
                away_team_id = away_team_id[0]

            if home_team_id > 0 and away_team_id > 0:
                command = "select WinningOdd,Bet from Games where LeagueId = '{0}' AND HomeTeamId = '{1}' AND AwayTeamId = '{2}';".format(league_id, home_team_id, away_team_id)
                games = cur.execute(command).fetchall()
                if games:
                    for (odd, bet) in games:
                        if odd == row_odd and bet == row_bet:
                            duplicate = True
                            break

    else:
        country_id = 0

    return(sport_id, country_id, league_id, home_team_id, away_team_id, duplicate)
    
def prepare_games(db_name, games_to_bet_file):
    games_df = pd.read_csv(games_to_bet_file)

    conn = sqlite3.connect(db_name)
    cur = conn.cursor()
    
    games_df['SportId'], games_df['CountryId'], games_df['LeagueId'], games_df['HomeTeamId'], games_df['AwayTeamId'], games_df['Duplicate'] = \
        zip(*games_df.apply(lambda row: create_columns(row, cur), axis=1))

    cur.close()
    conn.close()

    games_df.dropna(axis=0, how='any', inplace=True)

    return games_df

def predict_results(db_name, games_to_bet_file):
    #games_df = prepare_games(db_name, games_to_bet_file)
    games_df = pd.read_csv('pred.csv', encoding="ISO-8859-1")
    
    games = games_df.as_matrix(features)

    reg = joblib.load('models/model.pkl')
    predictions = reg.predict(games)
    probabilities = reg.predict_proba(games)
    draw = []
    home = []
    away = []
    for d, h, a in probabilities:
        draw.append(d)
        home.append(h)
        away.append(a)

    games_df['Prediction'] = predictions.tolist()
    games_df['HomeProba'] = home
    games_df['DrawProba'] = draw
    games_df['AwayProba'] = away

    games_df.to_csv('pred.csv', index=False)

def analyse_data(data, bet_name, winner_name, prediction_name, bet):
    data_count = data.shape[0]
    if data_count == 0:
        return

    bets = data[data[bet_name] == bet]
    bets_count = bets.shape[0]
    wins = data[data[winner_name] == bet]
    wins_count = wins.shape[0]
    predictions = data[data[prediction_name] == bet]
    predictions_count = predictions.shape[0]    
    
    bet_winner = data[(data[bet_name] == bet) & (data[winner_name] == bet)].shape[0]
    prediction_winner = data[(data[prediction_name] == bet) & (data[winner_name] == bet)].shape[0]
    
    print('{0} - {1}, {2} - {3}, {4} - {5}'.format(bet_name, bets_count, winner_name, wins_count, prediction_name, predictions_count))
    print('{} / data count'.format(bet_name))
    print(bets_count * 1.0 / data_count)
    if bets_count > 0:    
        print('({0} and {1}) / {1}'.format(winner_name, bet_name))
        print(bet_winner * 1.0 / bets_count)
    
    if predictions_count > 0:
        print('({0} and {1}) / {1}'.format(winner_name, prediction_name))
        print(prediction_winner * 1.0 / predictions_count)
    print()

def analyse_result():
    all_data = pd.read_csv('pred.csv', encoding="ISO-8859-1")
    # no duplicates
    all_data = all_data[~all_data['Duplicate']]
    #all_data = all_data[all_data['WinningOdd'] <= 1.35]
    # known teams
    #all_data = all_data[(all_data['HomeTeamId'] > 0) & (all_data['AwayTeamId'] > 0)]

    for sport_id, sport in sports:        
        data = all_data[all_data['SportId'] == sport_id]

        print(sport)
        
        data_count = data.shape[0]
        print('data count')
        print(data_count)
        print()
        
        print('{0} - {1}'.format('Winner', 'Prediction'))
        print(pd.crosstab(data['Winner'], data['Prediction'], rownames=['True'], colnames=['Predicted'], margins=True))
        print()

        analyse_data(data, 'Bet', 'Winner', 'Prediction', 1)
        analyse_data(data, 'Bet', 'Winner', 'Prediction', 0)
        analyse_data(data, 'Bet', 'Winner', 'Prediction', 2)        
    
if __name__ == '__main__':
    # db = os.path.abspath(os.path.join(os.path.dirname(__file__),\
    #                         os.pardir, 'ArchiveData.db'))
    
    # games_file = os.path.abspath(os.path.join(os.path.dirname(__file__),\
    #                        os.pardir, 'OddsScrapper', 'Archive', 'recentdata.csv'))

    # predict_results(db, games_file)
    predict_results('', '')
    analyse_result()

    pass