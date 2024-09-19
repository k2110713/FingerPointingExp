import pandas as pd
from statsmodels.stats.multicomp import pairwise_tukeyhsd
import statsmodels.api as sm
from statsmodels.formula.api import ols

# ファイルパスのリスト
files = ["output/ElapsedMilliseconds.csv", "output/ErrorRatio.csv"]
output_files = ["output/TukeyHSDResults_ElapsedMilliseconds.csv", "output/TukeyHSDResults_ErrorRatio.csv"]

for file_path, output_file in zip(files, output_files):
    # CSVファイルからデータを読み込む
    df_loaded = pd.read_csv(file_path)
    df_melt = pd.melt(df_loaded, var_name='Task', value_name='Value')

    # ANOVAモデルをフィット
    model = ols('Value ~ C(Task)', data=df_melt).fit()

    # TukeyのHSD検定を実行
    tukey_result = pairwise_tukeyhsd(endog=df_melt['Value'], groups=df_melt['Task'], alpha=0.05)

    # 結果をDataFrameに変換
    tukey_summary = pd.DataFrame(data=tukey_result._results_table.data[1:], columns=tukey_result._results_table.data[0])

    # 結果をCSVファイルに保存
    tukey_summary.to_csv(output_file, index=False, encoding='utf-8-sig')  # UTF-8 BOM付きで保存

    # 結果の表示（オプション）
    print(f'Tukey HSD results for {file_path} saved to {output_file}')
