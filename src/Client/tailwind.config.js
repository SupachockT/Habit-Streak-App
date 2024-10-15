/** @type {import('tailwindcss').Config} */
module.exports = {
    mode: "jit",
    content: [
        "./index.html",
        "./**/*.{fs,js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                davygray : '#5A5353',
                mountbattenpink : '#A07178',
                paledogwood : '#E6CCBE',
                chineseviolet : '#776274',
                sage : '#C8CC92'
            }
        },
    },
    plugins: [
        require('daisyui'),
    ]
}
