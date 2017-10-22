import pandas as pd
from sklearn.metrics import classification_report, confusion_matrix, roc_curve
from sklearn.model_selection import cross_val_score, train_test_split, KFold, GridSearchCV
from sklearn.externals import joblib
from sklearn.pipeline import make_pipeline
from sklearn.linear_model import SGDClassifier
from sklearn.preprocessing import StandardScaler, OneHotEncoder

features = ['Bet', 'HomeTeamIndex', 'AwayTeamIndex', 'HomeOdd', 'AwayOdd', 'IsPlayoffs', 'Year', 'Month', 'Day'] # 
label = 'Winner'

def find_best_clfs(data):
    X = data.as_matrix(features)
    y = data[label].values

    # standardize
    mean = X.mean(axis=0)
    std = X.std(axis=0)
    X = (X - mean) / std

    print('SGD')
    reg = SGDClassifier(max_iter=1000, tol=1e-4, random_state=42)
    pipe = make_pipeline(reg)
    parameters = {"sgdclassifier__loss":['hinge', 'log', 'modified_huber'], 
        "sgdclassifier__penalty":['l1', 'l2'],
        "sgdclassifier__learning_rate":['constant', 'optimal', 'invscaling'], 
        "sgdclassifier__eta0":[1, 0.1, 0.01],
        "sgdclassifier__alpha":[0.0001, 0.001, 0.01, 0.1]}
    clf = GridSearchCV(pipe, parameters, n_jobs= 2)
    clf.fit(X, y)
    model = clf.best_estimator_
    print(model)

def train_model(data, save_model):
    X = data.as_matrix(features)
    y = data[label].values
    
    # standardize
    mean = X.mean(axis=0)
    std = X.std(axis=0)
    print('mean {0}'.format(mean))
    print('std {0}'.format(std))

    features_train, features_test, labels_train, labels_test = train_test_split(X, y, test_size = 0.2, random_state = 0)
    
    pred = pd.Series(labels_test)
    result = pd.DataFrame(pred, columns=['Winner'])
    result['Bet'] = pd.Series(features_test[:, 0]).astype(int)
    result['HomeOdd'] = pd.Series(features_test[:, 3])
    result['AwayOdd'] = pd.Series(features_test[:, 4])

    features_train = (features_train - mean) / std
    features_test = (features_test - mean) / std

    reg = SGDClassifier(max_iter=1000, tol=1e-4, random_state=42, 
        loss="log", penalty='l1', learning_rate="optimal", eta0=1, alpha=0.001)
    reg.fit(features_train, labels_train)
    
    print('Trained')
    if save_model:
        joblib.dump(reg, 'models/model_nba.pkl')
    
    prediction = reg.predict(features_test)
    win_proba = []
    probabilities = reg.predict_proba(features_test)
    # interested only in probability of predicted result
    for i, proba in enumerate(probabilities):
        predict = prediction[i]
        if predict == 2:
            predict = 1
        win_proba.append(proba[predict])

    result['SGD'] = prediction.tolist()
    result['SGD_proba'] = win_proba

    print('Predicted')
    y_true = pd.Series(labels_test)
    y_pred = pd.Series(prediction)
    print(pd.crosstab(y_true, y_pred, rownames=['True'], colnames=['Predicted'], margins=True))
    print()
    print(confusion_matrix(labels_test, prediction))
    print()
    print(classification_report(labels_test, prediction))
    print()
    result.to_csv('test_predictions_nba.csv', index=False)

if __name__ == '__main__':
    db_data = pd.read_csv('archive.csv')
    db_data = db_data[db_data["LeagueId"] == 3026]

    train_model(db_data, False)
    # find_best_clfs(db_data)
    print('Done')