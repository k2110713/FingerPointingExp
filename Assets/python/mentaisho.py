import numpy as np

def calculate_symmetric_point(a, b, c, x):
    # 点 A, B, C の座標
    A = np.array(a)
    B = np.array(b)
    C = np.array(c)

    # 点 X の座標
    X = np.array(x)

    # ベクトル AB と AC を計算
    AB = B - A
    AC = C - A

    # 平面の法線ベクトルを計算 (AB と AC の外積)
    normal_vector = np.cross(AB, AC)

    # 平面の方程式の係数
    n1, n2, n3 = normal_vector
    d = -np.dot(normal_vector, A)

    # 点 X から平面への垂線の足 Q の座標を計算
    t = -(np.dot(normal_vector, X) + d) / (n1**2 + n2**2 + n3**2)
    Q = X + t * normal_vector

    # 対称な点 X' の座標を計算
    X_prime = 2 * Q - X

    return X_prime

# 入力
a = [-561.7103059, 964.9160064, 1853.637644]  # 点 A の座標
b = [-903.4780796, 965.3472271, 1864.978022]  # 点 B の座標
c = [-744.3455759, 1281.479441, 2197.401432]  # 点 C の座標
x = [-735.131386, 721.5947649, 1937.640436]  # 点 X の座標

# 対称な点 X' の座標を計算
x_prime = calculate_symmetric_point(a, b, c, x)
print("点 X' の座標:", x_prime)
