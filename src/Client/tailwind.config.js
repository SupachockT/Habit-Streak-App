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
                cp1 : '#343131',
                cp2 : '#A04747',
                cp3 : '#D8A25E',
                cp4 : '#EEDF7A'
            }
        },
    },
    plugins: [
        require('daisyui'),
    ]
}
