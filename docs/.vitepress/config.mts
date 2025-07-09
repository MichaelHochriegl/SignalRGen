import {defineConfig} from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
    title: "SignalRGen",
    description: "A dev-friendly experience to SignalR communication",
    themeConfig: {
        // https://vitepress.dev/reference/default-theme-config
        logo: 'logo.webp',
        nav: [
            {text: 'Home', link: '/'},
        ],

        sidebar: [
            {
                text: 'Introduction',
                items: [
                    {text: 'What is SignalRGen?', link: '/guide/what-is-signalrgen'},
                    {text: 'Getting Started', link: '/guide/getting-started'},
                    {text: 'Detailed Tutorial', link: '/guide/detailed-tutorial'},
                ]
            },
            {
                text: 'Hub Contracts',
                items: [
                    {text: 'Hub Interface Definition', link: '/hub-contracts/hub-interface-definition'},
                ]
            },
            {
                text: 'Client-Side Usage',
                items: [
                    {text: 'Generated Hub Client', link: '/client-side-usage/generated-hub-clients'},
                    {
                        text: 'Configuration', link: '/client-side-usage/configuration/config-overview',
                        collapsed: true,
                        items: [
                            {text: 'Global', link: '/client-side-usage/configuration/config-global'},
                            {text: 'Per-Hub', link: '/client-side-usage/configuration/config-per-hub'},
                        ]
                    },
                ]
            },
        ],

        socialLinks: [
            {icon: 'github', link: 'https://github.com/MichaelHochriegl/SignalRGen'}
        ]
    }
})
