# -*- coding: utf-8 -*-
"""A module that creates trained sklear models for predicting data.
"""
import pandas as pd
import numpy as np
from sklearn.metrics import classification_report
from model_builder import label, sports
import matplotlib.pyplot as plt
import matplotlib
matplotlib.style.use('ggplot')

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

def find_league_averages_for_data(data, out_file, bet_column, min_odd, max_odd):
    min_proba = 1.0/min_odd
    
    for league_id in data['LeagueId'].unique():
        league_df = data[data['LeagueId'] == league_id]
        for bet in [0,1,2]:
            bets = league_df[(league_df[bet_column] == bet)]
            no_bets = bets.shape[0]
            if no_bets < 1:
                continue
            no_wins = bets[bets[label] == bets[bet_column]].shape[0]
            precision = 1.0 * no_wins / no_bets
            avg_odd = 0
            if bet == 0:
                avg_odd = bets['DrawOdd'].mean()
            elif bet == 1:
                avg_odd = bets['HomeOdd'].mean()
            elif bet == 2:
                avg_odd = bets['AwayOdd'].mean()
            if no_bets < 100 or precision <= min_proba:
                continue
            out_file.write('{0},{1},{2},{3:.2f},{4:.2f},{5},{6}\n'.format(league_id, no_bets, bet, min_odd, max_odd, precision, avg_odd))

def find_league_averages(data, test_column = 'Bet'):
    odd = 1.0
    step = 0.2
    file_name = 'AnalysedFiles/best_leagues.csv'
    with open(file_name, mode='w') as out_file:
        out_file.write('LeagueId,NoData,Bet,MinOdd,MaxOdd,Score,AvgOdd\n')
        while (odd <= 3):
            data_odd = data[
                ((data[test_column] == 1) & (data['HomeOdd'] >= odd) & (data['HomeOdd'] < odd + step)) |
                ((data[test_column] == 2) & (data['AwayOdd'] >= odd) & (data['AwayOdd'] < odd + step)) |
                ((data[test_column] == 0) & (data['DrawOdd'] >= odd) & (data['DrawOdd'] < odd + step))
            ]
            min_odd = odd
            if odd == 1:
                min_odd = 1.01
            find_league_averages_for_data(data_odd, out_file, test_column, min_odd, odd + step)
            odd += step

def plot_histograms(df):
    #df = df[df['Bet'] == df[label]]
    #df = df[df['HomeOdd'] < 3]
    #df = df[df['SportId'] == 9]
    #df.plot().hist(column='WinningOdd',by='SportId', bins=10)
    pd.DataFrame.hist(df[(df['Bet'] == 1)], column='HomeOdd', by='SportId', bins=20)
    pd.DataFrame.hist(df[(df['Bet'] == 2)], column='AwayOdd', by='SportId', bins=20)
    plt.show()
def plot_data(data, axis_x, plot, winner_selector, no_bins = 10):
    arguments = []
    values = []
    counts = []
    
    # get steps
    minx = data[axis_x].min()
    maxx = data[axis_x].max()
    steps, step_size = np.linspace(minx, maxx, no_bins, endpoint=False, retstep=True)
    for step in steps.tolist():
        # get data from range
        df = data[(data[axis_x] >= step) & (data[axis_x] < step + step_size)]
        if df.shape[0] <= 0:
            continue
        whole_count = df.shape[0]
        # count the number of correct predictions
        win_count = df[winner_selector(df)].shape[0]
        
        arguments.append(step + step_size/2)
        values.append(1.0 * win_count / whole_count)
        counts.append(whole_count)

    rects = plot.bar(arguments, values, step_size)
    
    # create some labels
    # rects = plot.patches
    for i, rect in enumerate(rects):
        label = '{0}'.format(counts[i])
        plot.text(rect.get_x() + rect.get_width()/2., 1.05 * rect.get_height(), label, ha='center', va='bottom')
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
        plot_data(data, 'AwayOdd', ax1, lambda x: ((x['Winner'] == x['Bet'])))

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
    #data_stats(db_data)
    find_league_averages(db_data)
    #plot_by_sport(db_data)
    print('Done')
