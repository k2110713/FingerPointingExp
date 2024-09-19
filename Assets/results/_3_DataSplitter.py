import pandas as pd

# CSVファイルを読み込む
df = pd.read_csv('output/merged_output.csv')

# エラーレートを計算する
df['ErrorRatio'] = (10 - df['CorrectCount']) / 10

# NameとModeごとのElapsedMillisecondsの平均を計算
elapsed_mean = df.groupby(['Name', 'Mode'])['ElapsedMilliseconds'].mean().unstack().reset_index()

# NameとModeごとのErrorRatioの平均を計算
error_ratio_mean = df.groupby(['Name', 'Mode'])['ErrorRatio'].mean().unstack().reset_index()

# カラム名を設定
elapsed_mean.columns = ['Name', 'Touch', 'Trigger P(Push)', 'Trigger T(Thumb)', 'Trigger C(Circle)']
error_ratio_mean.columns = ['Name', 'Touch', 'Trigger P(Push)', 'Trigger T(Thumb)', 'Trigger C(Circle)']

# 結果をそれぞれのCSVファイルに保存
elapsed_mean.to_csv('output/ElapsedMilliseconds.csv', index=False)
error_ratio_mean.to_csv('output/ErrorRatio.csv', index=False)

print("計算が完了しました。結果はoutputフォルダ内のElapsedMilliseconds.csvとErrorRatio.csvに保存されました。")
