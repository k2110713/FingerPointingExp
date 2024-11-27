import pandas as pd
import os

# カレントディレクトリ内のファイルをリストアップ
directory = '.'  # カレントディレクトリ
files = [f for f in os.listdir(directory) if f.endswith('.csv') and not f.endswith('_info.csv')]

# 必須ヘッダー
required_headers = {'TaskNumber', 'ElapsedTime'}

# ヘッダーが不足しているファイルを記録するリスト
missing_header_files = []

# 各ファイルに対して処理を実行
for file_name in files:
    file_path = os.path.join(directory, file_name)
    
    # ファイルを読み込む
    try:
        data = pd.read_csv(file_path)
        headers = set(data.columns.tolist())
        
        # 必要なヘッダーが揃っているか確認
        if not required_headers.issubset(headers):
            missing_header_files.append((file_name, headers))
    except Exception as e:
        # CSVファイルの読み込みに失敗した場合も記録
        missing_header_files.append((file_name, f"読み込みエラー: {str(e)}"))

# 結果を出力
if missing_header_files:
    print("ヘッダーが不足している、またはエラーが発生したファイル:")
    for file, headers in missing_header_files:
        print(f"ファイル: {file}, ヘッダー: {headers}")
else:
    print("すべてのファイルに必要なヘッダーが揃っています。")
