# WelchsANOVABoth.py
import pandas as pd
import statsmodels.api as sm
from statsmodels.formula.api import ols
from statsmodels.stats.anova import anova_lm

# ファイルパスのリスト
files = ["output/ElapsedMilliseconds.csv", "output/ErrorRatio.csv"]

for file in files:
    # CSVファイルからの読み込み
    df_loaded = pd.read_csv(file)
    df_melt = pd.melt(df_loaded, var_name='Task', value_name='Value')

    # オルディナリー・リーストスクエア（OLS）モデルの設定
    model = ols('Value ~ C(Task)', data=df_melt).fit()

    # ANOVAテーブルの計算
    anova_results = anova_lm(model, typ=2)  # typ=2で等分散を前提としない

    # 結果の表示
    print(f'ANOVA result for {file}:')
    print(anova_results)
