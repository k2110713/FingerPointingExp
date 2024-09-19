import pandas as pd
import glob

# カレントディレクトリ内の全てのCSVファイルを検索
for file_path in glob.glob('*.csv'):
    # CSVファイルを読み込む
    df = pd.read_csv(file_path)
    
    # TestNumberが0以下の行を削除
    df = df[df['TestNumber'] > 0]
    
    # TestNumberが1から5までのリストを作成
    required_test_numbers = [1, 2, 3, 4, 5]
    
    # ファイル内のTestNumberを取得し、ユニークな値のリストを作成
    existing_test_numbers = df['TestNumber'].unique().tolist()
    
    # 必要なTestNumberがすべて存在するかチェック
    if not all(num in existing_test_numbers for num in required_test_numbers):
        # 必要なTestNumberが1つでも欠けている場合、すべてのTestNumberに1を加える
        df['TestNumber'] = df['TestNumber'] + 1
        print(f"{file_path} has been updated. All TestNumbers incremented by 1.")
    else:
        print(f"{file_path}: All required TestNumbers are present. No changes made.")

    # ファイルを上書き保存
    df.to_csv(file_path, index=False)
