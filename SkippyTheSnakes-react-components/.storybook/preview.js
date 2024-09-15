export const parameters = {
    actions: { argTypesRegex: "^on[A-Z].*" },
    controls: {
        matchers: {
            color: /(background|color)$/i,
            date: /Date$/,
        },
    },
    backgrounds: {
        default: 'night-1',
        values: [
            {
                name: 'night-1',
                value: '#2E3440',
            },
        ],
    },
};
