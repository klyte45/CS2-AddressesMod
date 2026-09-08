const starPoints = [
    [0, 0.46875],
    [0.125, 0.11458333333333333],
    [0.5, 0.11458333333333333],
    [0.20833333333333334, -0.11458333333333333],
    [0.3125, -0.46875],
    [0, -0.2604166666666667],
    [-0.3125, -0.46875],
    [-0.20833333333333334, -0.11458333333333333],
    [-0.5, 0.11458333333333333],
    [-0.125, 0.11458333333333333],
];

const starAtPosition = (x: number, y: number, size = 1) =>
    starPoints.map((point) => [point[0] * size + x, point[1] * size + y]);

export function getStarPathD(x: number, y: number, size: number) {
    return `M${starAtPosition(x, y, size).map((point) => point.map((p) => p.toFixed(3)).join(" ")).join(" L ")}z`;
}
