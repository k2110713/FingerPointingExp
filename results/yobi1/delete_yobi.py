import os

# 対象ディレクトリのパスを指定
directory = 'yobi1'

# ディレクトリ内のすべてのファイルを走査
for filename in os.listdir(directory):
    # ファイルがCSVファイルの場合
    if filename.endswith('.csv') and 'yobi_' in filename:
        # 新しいファイル名を作成
        new_filename = filename.replace('yobi_', '')
        # ファイルのパスを更新
        old_file = os.path.join(directory, filename)
        new_file = os.path.join(directory, new_filename)
        # ファイル名を変更
        os.rename(old_file, new_file)
        print(f"Renamed: {filename} -> {new_filename}")

print("ファイル名の変更が完了しました。")
