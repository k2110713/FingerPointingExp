import pandas as pd
import glob
import os
import re

# 出力ファイルのパス
output_csv = 'output/merged_output.csv'

# 結果を格納するための空のDataFrame
result_df = pd.DataFrame()

# カレントディレクトリ内の全てのCSVファイルを検索
for file_path in glob.glob('*.csv'):
    # CSVファイルを読み込む
    df = pd.read_csv(file_path)
    
    # TestNumberが0以下の行を削除
    df = df[df['TestNumber'] > 0]
    
    # Countが10の行をフィルタリング
    df_filtered = df[df['Count'] == 10].copy()  # Make a copy to avoid SettingWithCopyWarning
    
    # ファイル名から名前、年月日、モードを抽出
    file_name = os.path.basename(file_path)
    match = re.match(r"(\D+)_(\d+)_(\d{8}).csv", file_name)
    if match:
        name = match.group(1)
        mode = int(match.group(2))  # Convert mode to integer explicitly
        date = match.group(3)
        
        # Use .loc to ensure we are modifying the DataFrame copy directly
        df_filtered.loc[:, 'Name'] = name
        df_filtered.loc[:, 'Date'] = date
        df_filtered.loc[:, 'Mode'] = mode  # Now assigning integer, compatible with int64 dtype
        
        # Select only the necessary columns
        df_filtered = df_filtered[['Name', 'Date', 'Mode', 'ElapsedMilliseconds', 'CorrectCount']]
        
        # Append the filtered data to the result DataFrame
        result_df = pd.concat([result_df, df_filtered], ignore_index=True)

# 新しいCSVファイルに保存
result_df.to_csv(output_csv, index=False)
