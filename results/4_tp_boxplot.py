import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import numpy as np

# CSVデータを読み込む
file_path = "processed_results_name_average.csv"  # 適宜ファイルパスを設定
data = pd.read_csv(file_path)

# スループットを計算
def calculate_throughput(row):
    if row['radius'] == 400:
        return np.log(67.5 / 18 + 1) / row['elapsedtime']
    elif row['radius'] == 800:
        return np.log(135 / 18 + 1) / row['elapsedtime']
    else:
        return None  # 予期しない値に対するエラーハンドリング

data['throughput'] = data.apply(calculate_throughput, axis=1)

# スループットを含むデータを新しいCSVファイルに保存
output_file = "processed_results_with_throughput.csv"
data.to_csv(output_file, index=False)

print(f"スループットを含むデータを '{output_file}' に保存しました。")

# 箱ひげ図を作成
plt.figure(figsize=(10, 6))
sns.boxplot(x='radius', y='throughput', hue='method', data=data)

# グラフのラベルとタイトルを設定
plt.xlabel('Radius', fontsize=12)
plt.ylabel('Throughput', fontsize=12)
plt.title('Boxplot of Throughput', fontsize=14)
plt.legend(title='Method')
plt.grid(True, linestyle="--", alpha=0.6)

# グラフを表示
plt.tight_layout()
plt.show()
