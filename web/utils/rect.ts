export interface IRect {
  top: number;
  right: number;
  bottom: number;
  left: number;
}

export function rectHasIntersection(a: IRect, b: IRect) {
  const top = Math.max(a.top, b.top);
  const bottom = Math.max(top, Math.min(a.bottom, b.bottom));
  if (bottom - top === 0) return false;

  const left = Math.max(a.left, b.left);
  const right = Math.max(left, Math.min(a.right, b.right));
  if (right - left === 0) return false;
  return true;
}

export function rectSize(rect: IRect) {
  return (rect.bottom - rect.top) * (rect.right - rect.left);
}
