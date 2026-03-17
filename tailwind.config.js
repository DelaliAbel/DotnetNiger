/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Pages/**/*.razor",
        "./Components/**/*.razor",
        "./Pages/*.razor",
        "./wwwroot/**/*.html",
        "./Layout/*.razor"
    ],
    theme: {
        extend: {
            keyframes:{
                modalEnter:{
                    '0%': {
                        transform: 'translateX(100%)',
                        opacity: '0'
                    },
                    '100%': {
                        transform: 'translateX(0)',
                        opacity: '1'
                    }
                },
                modalExit:{
                    '0%': {
                        transform: 'translateX(0)',
                        opacity: '1'
                    },
                    '100%': {
                        transform: 'translateX(100%)',
                        opacity: '0'
                    }
                }
            },
            animation:{
                modalEnter : 'modalEnter 0.35s ease-out forwards',
                modalExit: 'modalExit 0.35s ease-in forwards'
            },
            colors: {
                // Mapper les couleurs de Tailwind à vos variables CSS
                primary: 'var(--color-primary)',
                secondary: 'var(--color-secondary)',
                text: 'var(--color-text)',
                hover: 'var(--color-hover)',
                active: 'var(--color-active)',
                focus: 'var(--color-focus)',
                white: 'var(--color-white)',
                fond_tranparent: 'var(--color-soft)'
            },
        },
    },
    plugins: [],
}