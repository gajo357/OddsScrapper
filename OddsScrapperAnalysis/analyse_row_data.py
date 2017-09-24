# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import os
import sqlite3
import pandas as pd
from sklearn.metrics import classification_report, precision_score
from model_builder import features, label, sports
import matplotlib.pyplot as plt
import matplotlib
matplotlib.style.use('ggplot')
from model_tester import plot_data

def data_stats(data):
    #data = data[data['SportId'] == 9]

    data_count = data.shape[0]
    print('data count')
    print(data_count)

    print('home bet')
    home_bets = data[(data['Bet'] == 1) & (data['HomeOdd'] >= 2)]
    print(classification_report(home_bets[label].values, home_bets['Bet'].values))

    print('away bet')
    away_bets = data[(data['Bet'] == 2) & (data['AwayOdd'] >= 2)]
    print(classification_report(away_bets[label].values, away_bets['Bet'].values))

    print('all bets')
    print(classification_report(data[label].values, data['Bet'].values))

def find_league_averages_for_data(data, file_name, bet_column):
    with open(file_name, mode='w') as out_file:
        out_file.write('LeagueId,NoData,Bet,Score\n')
        for league_id in data['LeagueId'].unique():
            league_df = data[data['LeagueId'] == league_id]
            for bet in [0,1,2]:
                bets = league_df[(league_df[bet_column] == bet)]
                no_bets = bets.shape[0]
                if no_bets < 1:
                    continue
                precision = precision_score(bets[label].values, bets[bet_column].values, average=None)
                if bet != 0 and len(precision) == 2:
                    precision = precision[bet - 1]
                elif len(precision) == 1:
                    precision = precision[0]
                else:
                    precision = precision[bet]

                out_file.write('{0},{1},{2},{3}\n'.format(league_id, no_bets, bet, precision))

def find_league_averages(data, test_column = 'Bet'):
    bet_column = 'Bet'
    # 1.5 <= x < 2  
    data15 = data[
        ((data[bet_column] == 1) & (data['HomeOdd'] >= 1.5) & (data['HomeOdd'] < 2)) |
        ((data[bet_column] == 2) & (data['AwayOdd'] >= 1.5) & (data['AwayOdd'] < 2)) |
        ((data[bet_column] == 0) & (data['DrawOdd'] >= 1.5) & (data['DrawOdd'] < 2))
     ]
    find_league_averages_for_data(data15, 'league_average_{}_15.csv'.format(test_column), test_column)
    
    # 1.25 <= x < 1.5
    data12 = data[
        ((data[bet_column] == 1) & (data['HomeOdd'] >= 1.25) & (data['HomeOdd'] < 1.5)) |
        ((data[bet_column] == 2) & (data['AwayOdd'] >= 1.25) & (data['AwayOdd'] < 1.5)) |
        ((data[bet_column] == 0) & (data['DrawOdd'] >= 1.25) & (data['DrawOdd'] < 1.5))
     ]
    find_league_averages_for_data(data12, 'league_average_{}_12.csv'.format(test_column), test_column)
    
    # 2 <= x 
    data2 = data[
        ((data[bet_column] == 1) & (data['HomeOdd'] >= 2)) |
        ((data[bet_column] == 2) & (data['AwayOdd'] >= 2)) |
        ((data[bet_column] == 0) & (data['DrawOdd'] >= 2))
     ]
    find_league_averages_for_data(data2, 'league_average_{}_2.csv'.format(test_column), test_column)
    
    # x < 1.2 
    data1 = data[
        ((data[bet_column] == 1) & (data['HomeOdd'] < 1.25)) |
        ((data[bet_column] == 2) & (data['AwayOdd'] < 1.25)) |
        ((data[bet_column] == 0) & (data['DrawOdd'] < 1.25))
     ]
    find_league_averages_for_data(data1, 'league_average_{}_1.csv'.format(test_column), test_column)

def plot_histograms(df):
    plt.figure()
    #df = df[df['Bet'] == df[label]]
    #df = df[df['HomeOdd'] < 3]
    #df = df[df['SportId'] == 9]
    #df.plot().hist(column='WinningOdd',by='SportId', bins=10)
    pd.DataFrame.hist(df[(df['Bet'] == 1)], column='HomeOdd', by='SportId', bins=20)
    pd.DataFrame.hist(df[(df['Bet'] == 2)], column='AwayOdd', by='SportId', bins=20)
    plt.show()
def plot_by_sport(all_data):
    for sport_id, sport in sports:
        df = all_data[all_data['SportId'] == sport_id]
        plot_id = sport_id
        i = 0         

        data = df[(df['Bet'] == 1)]
        plt.figure(i)
        i += 1
        ax1 = plt.subplot(4, 3, plot_id)
        ax1.set_title('{0} {1}'.format(sport, sport_id))
        plot_data(data, 'HomeOdd', ax1, lambda x: ((x['Winner'] == x['Bet'])))
        
        plt.figure(i)
        i += 1
        ax1 = plt.subplot(4, 3, plot_id)
        ax1.set_title('{0} {1}'.format(sport, sport_id))
        plot_data(data, 'HomeOdd', ax1, lambda x: ((x['Bet'] != x['Winner'])))
        
        data = df[(df['Bet'] == 2)]
        plt.figure(i)
        i += 1
        ax1 = plt.subplot(4, 3, plot_id)
        ax1.set_title('{0} {1}'.format(sport, sport_id))
        plot_data(data, 'AwayOdd', ax1, lambda x: ((x['Winner'] == x['NB'])))

        plt.figure(i)
        i += 1
        ax1 = plt.subplot(4, 3, plot_id)
        ax1.set_title('{0} {1}'.format(sport, sport_id))
        plot_data(data, 'AwayOdd', ax1, lambda x: ((x['Bet'] != x['Winner'])))
        
    plt.show()

if __name__ == '__main__':
    db_data = pd.read_csv('archive.csv')
    
    #db_data = db_data[db_data['SportId'] == 9]
    #plot_histograms(db_data)
    # data_stats(db_data)
    # find_league_averages(db_data, 'NB')
    plot_by_sport(db_data)
    #analyse_all()
    pass
