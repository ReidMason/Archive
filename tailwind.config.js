module.exports = {
  mode: 'jit',
  purge: [
    './src/**/*.html',
    './src/**/*.js',
    './src/**/*.jsx',
    './src/**/*.ts',
    './src/**/*.tsx',
    './public/index.html'
  ],
  darkMode: false, // or 'media' or 'class'
  theme: {
    extend: {
      colors: {
        night: {
          1: "#2E3440",
          2: "#3B4252",
          3: "#434C5E",
          4: "#4C566A",
        },
        snow: {
          1: "#D8DEE9",
          2: "#E5E9F0",
          3: "#ECEFF4"
        },
        frost: {
          1: "#8FBCBB",
          2: "#88C0D0",
          3: "#81A1C1",
          4: "#5E81AC"
        },
        red: "#BF616A",
        orange: "#D08770",
        yellow: "#EBCB8B",
        green: "#A3BE8C",
        purple: "#B48EAD"
      }
    },
  },
  variants: {
    extend: {},
  },
  plugins: [],
}
