# ClassicANOVABoth.py
import pandas as pd
import scipy.stats as stats

# ファイルパスのリスト
files = ["output/ElapsedMilliseconds.csv", "output/ErrorRatio.csv"]

for file in files:
    # CSVファイルからの読み込み
    df_loaded = pd.read_csv(file)

    # 一要因分散分析（ANOVA）
    anova_result = stats.f_oneway(*[df_loaded[col].dropna() for col in df_loaded.columns])

    # ANOVA結果の表示
    print(f'Classic ANOVA result for {file}: F-statistic = {anova_result.statistic:.2f}, p-value = {anova_result.pvalue:.4f}')
