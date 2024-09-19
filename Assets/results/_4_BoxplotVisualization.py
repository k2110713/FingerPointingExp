import pandas as pd
import matplotlib.pyplot as plt
import matplotlib

# MS Gothic フォントの設定
matplotlib.rcParams['font.family'] = 'MS Gothic'
matplotlib.rcParams['font.sans-serif'] = ['MS Gothic']

# フォントサイズの設定
plt.rcParams.update({'font.size': 32})

# ファイルパスのリスト
files = ["output/ElapsedMilliseconds.csv", "output/ErrorRatio.csv"]
labels = ["Elapsed Time (s)", "Error Ratio"]

# それぞれのデータセットのグラフを別々に描画
for file, label in zip(files, labels):
    # CSVファイルからの読み込み
    df_loaded = pd.read_csv(file)
    
    # Name列を無視
    df_loaded = df_loaded.drop(columns=['Name'])
    
    # ElapsedTimeを秒に変換（もしmsからsに変換が必要な場合）
    if label == "Elapsed Time (s)":
        df_loaded = df_loaded / 1000

    # 箱ひげ図の描画
    plt.figure(figsize=(10, 8))  # グラフサイズの設定
    boxplot = plt.boxplot(
        [df_loaded[col].dropna() for col in df_loaded.columns], 
        labels=[col.replace('(', '\n(') for col in df_loaded.columns], 
        patch_artist=False,
        medianprops=dict(color='black')  # 中央値の線を黒に設定
    )
    
    # 平均値の計算とプロット
    for i, col in enumerate(df_loaded.columns):
        data = df_loaded[col].dropna()
        mean = data.mean()
        print(f"平均値 ({label}) - {col}: {mean:.2f}")
        plt.plot(i + 1, mean, 'r^', markersize=10, label='Mean' if i == 0 else "")  # 平均値を赤い三角形でプロット
    
    plt.ylabel(label)  # 縦軸のラベル設定
    plt.legend()  # 凡例の追加
    plt.grid(True)

    # 縦軸の上限設定
    if label == "Elapsed Time (s)":
        plt.ylim(0, 30)
    elif label == "Error Ratio":
        plt.ylim(0, 1)

    plt.show()  # グラフの表示
