import pandas as pd
import os

# カレントディレクトリ内のファイルをリストアップ
directory = '.'  # カレントディレクトリ
files = [f for f in os.listdir(directory) if f.endswith('.csv') and not f.endswith('_info.csv')]

# 結果を保存するリスト
results = []

# 各ファイルに対して処理を実行
for file_name in files:
    file_path = os.path.join(directory, file_name)
    data = pd.read_csv(file_path)

    # ElapsedTimeの差を計算
    data['ElapsedDiff'] = data.groupby('TaskNumber')['ElapsedTime'].diff()

    # NaNを除外
    data.dropna(subset=['ElapsedDiff'], inplace=True)

    # TaskNumberの最大値を取得
    max_task_number = data['TaskNumber'].max()

    # 後半のデータをフィルタリング (TaskNumber >= 9 のデータ)
    filtered_data = data[data['TaskNumber'] >= 9]

    # 後半のデータが存在する場合のみ平均差を計算
    if not filtered_data.empty:
        average_diff = filtered_data['ElapsedDiff'].mean()
    else:
        average_diff = None  # データがない場合はNoneを記録

    # ファイル名と平均差を保存
    results.append((file_name, average_diff))

# 結果の出力
for result in results:
    print(f"{result[0]}: 後半データの平均ElapsedTimeの差 = {result[1]}")

# 結果をDataFrameに変換
processed_results = []

for result in results:
    file_name, elapsed_time = result
    # ファイル名からradiusとmethodを抽出
    parts = file_name.split('_')
    name = parts[0]  # 最初の_までをnameとする
    radius = int(parts[2])  # 400 or 800
    method = parts[3].lower().replace(".csv", "")  # pointing or touch
    processed_results.append({"name": name, "radius": radius, "method": method, "elapsedtime": elapsed_time})

# DataFrameに変換
df = pd.DataFrame(processed_results)

# radius, methodでソート
df_sorted = df.sort_values(by=['radius', 'method'], ascending=[True, True])

# 結果を新しいCSVファイルに保存
output_file = 'processed_results_filtered.csv'
df_sorted.to_csv(output_file, index=False)

print(f"結果を '{output_file}' に保存しました。")
