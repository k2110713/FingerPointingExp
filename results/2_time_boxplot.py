import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

# CSVデータを読み込む
file_path = "processed_results_name_average.csv"  # 適宜ファイルパスを設定
data = pd.read_csv(file_path)

# 箱ひげ図を作成
plt.figure(figsize=(10, 6))
sns.boxplot(x='radius', y='elapsedtime', hue='method', data=data)

# グラフのラベルとタイトルを設定
plt.xlabel('Radius', fontsize=12)
plt.ylabel('Elapsed Time', fontsize=12)  # 平均ではなくそのままの値
plt.title('Boxplot of Elapsed Time', fontsize=14)
plt.legend(title='Method')
plt.grid(True, linestyle="--", alpha=0.6)

# グラフを表示
plt.tight_layout()
plt.show()
