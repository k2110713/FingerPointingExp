import pandas as pd
import os

# カレントディレクトリ内のファイルをリストアップ
directory = 'exp02'  # カレントディレクトリ
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

    # 後半のデータをフィルタリング (TaskNumber >= 9 のデータ)
    filtered_data = data[data['TaskNumber'] >= 9]

    # 結果を保存
    for _, row in filtered_data.iterrows():
        # ファイル名からradiusとmethodを抽出
        parts = file_name.split('_')
        name = parts[0]  # 最初の_までをnameとする
        radius = int(parts[2])  # 400 or 800
        method = parts[3].lower().replace(".csv", "")  # pointing or touch
        results.append({
            "name": name,
            "radius": radius,
            "method": method,
            "elapsedtime": row['ElapsedDiff']
        })

# 結果をDataFrameに変換
df = pd.DataFrame(results)

# 人ごとの平均を計算
name_grouped = df.groupby(['name', 'radius', 'method'])['elapsedtime'].mean().reset_index()

# radius, methodでソート
df_sorted = name_grouped.sort_values(by=['radius', 'method'], ascending=[True, True])

# 結果を新しいCSVファイルに保存
output_file = 'processed_results_name_average.csv'
df_sorted.to_csv(output_file, index=False)

print(f"結果を '{output_file}' に保存しました。")
