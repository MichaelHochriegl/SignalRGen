import {defineConfig} from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "SignalRGen",
  description: "A dev-friendly experience to SignalR communication",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    logo: 'logo.webp',
    nav: [
      { text: 'Home', link: '/' },
    ],

    sidebar: [
      {
        text: 'Introduction',
        items: [
          { text: 'What is SignalRGen?', link: '/guide/what-is-signalrgen' },
          { text: 'Getting Started', link: '/guide/getting-started' },
        ]
      },
      {
        text: 'Configuration',
        items: [
          { text: 'Config-Overview', link: '/configuration/config-overview' },
          { text: 'Global', link: '/configuration/config-global' },
          { text: 'Per-Hub', link: '/configuration/config-per-hub' },
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/MichaelHochriegl/SignalRGen' }
    ]
  }
})
