import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import string

# データを読み込む
file_path = "processed_results_name_average.csv"  # 適宜ファイルパスを設定
data = pd.read_csv(file_path)

# nameを大文字アルファベットに変換して匿名化
unique_names = sorted(data['name'].unique())  # 一意の名前を取得
name_mapping = {name: chr(65 + i) for i, name in enumerate(unique_names)}  # 'A', 'B', 'C', ...
data['name'] = data['name'].map(name_mapping)

# グラフを描画
plt.figure(figsize=(14, 8))

# `radius`と`method`の組み合わせごとにサブプロットを作成
for (radius, method), group in data.groupby(['radius', 'method']):
    plt.plot(group['name'], group['elapsedtime'], marker='o', label=f"Radius: {radius}, Method: {method}")

# グラフの設定
plt.xlabel("Name (Anonymized)", fontsize=12)
plt.ylabel("Elapsed Time (Mean)", fontsize=12)
plt.title("Individual Results of Elapsed Time", fontsize=14)
plt.xticks(rotation=45, fontsize=10)
plt.legend(title="Condition", fontsize=10)
plt.grid(True, linestyle="--", alpha=0.6)

# グラフを表示
plt.tight_layout()
plt.show()

# 匿名化マッピングを表示（確認用）
print("Anonymous Name Mapping:")
print(name_mapping)
