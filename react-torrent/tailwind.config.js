module.exports = {
  mode: 'jit',
  theme: {
    extend: {
      colors: {
        // Add nord theme colour pallete
        // https://www.nordtheme.com/docs/colors-and-palettes
        nord: {
          0: "#2e3440",
          1: "#3b4252",
          2: "#434c5e",
          3: "#4c566a",
          4: "#d8dee9",
          5: "#e5e9f0",
          6: "#eceff4",
          7: "#8fbcbb",
          8: "#88c0d0",
          9: "#81a1c1",
          10: "#5e81ac",
          11: "#bf616a",
          12: "#d08770",
          13: "#ebcb8b",
          14: "#a3be8c",
          15: "#b48ead"
        },
        green: {
          light: "hsl(92, 70%, 65%)"
        }
      },
    }
  },
  variants: {
    extend: {
      // cursor: ['disabled'],
      // textColor: ['active'],
      // backgroundColor: ['active']
    }
  },
  plugins: [],
  purge: {
    // Filenames to scan for classes
    content: [
      './src/**/*.html',
      './src/**/*.js',
      './src/**/*.jsx',
      './src/**/*.ts',
      './src/**/*.tsx',
      './public/index.html',
    ],
    // Options passed to PurgeCSS
    options: {
      // Whitelist specific selectors by name
      // safelist: [],
    },
  },
}
