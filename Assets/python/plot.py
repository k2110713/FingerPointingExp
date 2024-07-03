import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

# Coordinates specified
coordinates = [
    [-561.7103059, 964.9160064, 1853.637644],  # Point A
    [-903.4780796, 965.3472271, 1864.978022],  # Point B
    [-744.3455759, 1281.479441, 2197.401432],  # Point C
    [-735.131386, 721.5947649, 1937.640436],  # Point X
    [-745.19146285, 1058.95492565, 1621.62921018],  # Point X'
]

fig = plt.figure()
ax = fig.add_subplot(111, projection="3d")

# Plot each point with a different color
colors = ["r", "g", "b", "y", "m"]
labels = ["A", "B", "C", "X", "X'"]

for coord, color, label in zip(coordinates, colors, labels):
    ax.scatter(*coord, color=color, label=label)

# Set labels and legend
ax.set_xlabel("X")
ax.set_ylabel("Y")
ax.set_zlabel("Z")
ax.legend()

# Display plot
plt.show()
