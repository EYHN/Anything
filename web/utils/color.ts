function clamp(num: number, min: number, max: number) {
  return Math.min(Math.max(min, num), max);
}

export class Color {
  constructor(readonly r: number = 0, readonly g: number = 0, readonly b: number = 0, readonly a: number = 1) {}
  static fromCSS(string: string) {
    if (!string) {
      throw new Error(`Invalid color input ${JSON.stringify(string)}'`);
    }

    const abbr = /^#([a-f0-9]{3,4})$/i;
    const hex = /^#([a-f0-9]{6})([a-f0-9]{2})?$/i;
    const rgba = /^rgba?\(\s*([+-]?\d+)\s*,\s*([+-]?\d+)\s*,\s*([+-]?\d+)\s*(?:,\s*([+-]?[\d.]+)\s*)?\)$/;
    const per = /^rgba?\(\s*([+-]?[\d.]+)%\s*,\s*([+-]?[\d.]+)%\s*,\s*([+-]?[\d.]+)%\s*(?:,\s*([+-]?[\d.]+)\s*)?\)$/;

    const rgb = [0, 0, 0, 1];
    let match;
    let i;
    let hexAlpha;

    if ((match = string.match(hex))) {
      hexAlpha = match[2];
      match = match[1];

      for (i = 0; i < 3; i++) {
        // https://jsperf.com/slice-vs-substr-vs-substring-methods-long-string/19
        const i2 = i * 2;
        rgb[i] = parseInt(match.slice(i2, i2 + 2), 16);
      }

      if (hexAlpha) {
        rgb[3] = parseInt(hexAlpha, 16) / 255;
      }
    } else if ((match = string.match(abbr))) {
      match = match[1];
      hexAlpha = match[3];

      for (i = 0; i < 3; i++) {
        rgb[i] = parseInt(match[i] + match[i], 16);
      }

      if (hexAlpha) {
        rgb[3] = parseInt(hexAlpha + hexAlpha, 16) / 255;
      }
    } else if ((match = string.match(rgba))) {
      for (i = 0; i < 3; i++) {
        rgb[i] = parseInt(match[i + 1], 0);
      }

      if (match[4]) {
        rgb[3] = parseFloat(match[4]);
      }
    } else if ((match = string.match(per))) {
      for (i = 0; i < 3; i++) {
        rgb[i] = Math.round(parseFloat(match[i + 1]) * 2.55);
      }

      if (match[4]) {
        rgb[3] = parseFloat(match[4]);
      }
    } else {
      throw new Error(`Invalid color input '${JSON.stringify(string)}'`);
    }

    for (i = 0; i < 3; i++) {
      rgb[i] = clamp(rgb[i], 0, 255);
    }
    rgb[3] = clamp(rgb[3], 0, 1);

    return new Color(rgb[0] / 255, rgb[1] / 255, rgb[2] / 255, rgb[3]);
  }

  withAlpha(a: number): Color {
    return new Color(this.r, this.g, this.b, a);
  }

  toCSS(): string {
    return `rgba(${(255 * this.r).toFixed()}, ${(255 * this.g).toFixed()}, ${(255 * this.b).toFixed()}, ${this.a.toFixed(2)})`;
  }
}
