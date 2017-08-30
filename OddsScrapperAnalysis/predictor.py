# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import os
import sqlite3
import pandas as pd
from sklearn.externals import joblib
from model_builder import features, label

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
    else:
        country_id = 0

    return(sport_id, country_id, league_id, home_team_id, away_team_id)
    
def prepare_games(db_name, games_to_bet_file):
    games_df = pd.read_csv(games_to_bet_file)

    conn = sqlite3.connect(db_name)
    cur = conn.cursor()
    
    games_df['SportId'], games_df['CountryId'], games_df['LeagueId'], games_df['HomeTeamId'], games_df['AwayTeamId'] = \
        zip(*games_df.apply(lambda row: create_columns(row, cur), axis=1))

    # games_df['HomeTeam'] = games_df['Participants'].apply(lambda x = get_participant(x, True))
    # games_df['AwayTeam'] = games_df['Participants'].apply(lambda x = get_participant(x, False))

    # command = "select Id from Sports where Name={0};"
    # games_df['SportId'] = games_df['Sport'].apply(lambda x = cur.execute(command.format(x)).fetchone())
    # command = "select Id from Countries where Name={0};"
    # games_df['CountryId'] = games_df['Country'].apply(lambda x = cur.execute(command.format(x)).fetchone())
    
    # command = "select Id from Leagues where Name={0},SportId={1},CountryId={2};"
    # games_df['LeagueId'] = games_df['League'].apply(lambda x = cur.execute(command.format(x)).fetchone())

    # command = "select Id from Teams where Name={0};"
    # games_df['HomeTeamId'] = games_df['HomeTeam'].apply(lambda x = cur.execute(command.format(x)).fetchone())
    # games_df['AwayTeamId'] = games_df['AwayTeam'].apply(lambda x = cur.execute(command.format(x)).fetchone())

    cur.close()
    conn.close()

    games_df.dropna(axis=0, how='any', inplace=True)
    return games_df

def predict_results(db_name, games_to_bet_file, date_of_bet):
    games_df = prepare_games(db_name, games_to_bet_file)
    #games_df = pd.read_csv('games.csv', encoding="ISO-8859-1")
    
    games = games_df.as_matrix(features)

    reg = joblib.load('models/model.pkl')
    predictions = reg.predict(games)
    probabilities = reg.predict_proba(games)

    games_df['Prediction'] = predictions.tolist()

    games_df.to_csv('pred_{}.csv'.format(date_of_bet), index=False)

if __name__ == '__main__':
    date_str = '31Aug2017'

    db = os.path.abspath(os.path.join(os.path.dirname(__file__),\
                            os.pardir, 'OddsWebsite', 'ArchiveData.db'))

    games_file = os.path.abspath(os.path.join(os.path.dirname(__file__),\
                            os.pardir, 'OddsScrapper', 'TommorowsGames', 'games_{}.csv'.format(date_str)))

    predict_results(db, games_file, date_str)

    pass
    
