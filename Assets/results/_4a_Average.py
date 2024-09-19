import pandas as pd

# CSVファイルの読み込み
df = pd.read_csv("output/merged_output.csv")

# (10 - CorrectCount)/10 の計算
df['ErrorRatio'] = (10 - df['CorrectCount']) / 10

# 名前とモードごとのElapsedMillisecondsの平均値を計算
elapsed_time_grouped = df.groupby(['Name', 'Mode'])['ElapsedMilliseconds'].mean().reset_index()
elapsed_time_grouped.columns = ['Name', 'Mode', 'AverageElapsedMilliseconds']

# 名前とモードごとのErrorRatioの平均値を計算
error_ratio_grouped = df.groupby(['Name', 'Mode'])['ErrorRatio'].mean().reset_index()
error_ratio_grouped.columns = ['Name', 'Mode', 'AverageErrorRatio']

# 平均値をそれぞれのCSVファイルに書き込み
elapsed_time_grouped.to_csv("output/elapsed_time_averages.csv", index=False)
error_ratio_grouped.to_csv("output/error_ratio_averages.csv", index=False)

# それぞれのCSVファイルの読み込み
elapsed_df = pd.read_csv("output/elapsed_time_averages.csv")
error_df = pd.read_csv("output/error_ratio_averages.csv")

# Modeでマージソート
merged_df = pd.merge(elapsed_df, error_df, on=['Name', 'Mode']).sort_values(by='Mode')

# マージされたデータフレームを新しいCSVファイルに書き込み
merged_df.to_csv("output/merged_sorted_averages.csv", index=False)

print("平均値をそれぞれのCSVファイルに書き込みました: output/elapsed_time_averages.csv, output/error_ratio_averages.csv")
print("モードでマージソートしたCSVファイルを作成しました: output/merged_sorted_averages.csv")
