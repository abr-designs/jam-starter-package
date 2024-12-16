import { defineConfig } from 'vitepress'
import { generateSidebar } from './sidebar-generator'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "Jam Starter Package",
  description: "Documentation",
  srcDir: './site', // Specify the location of Markdown files
  base: '/', // needs to be changed for building on github
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Examples', link: '/markdown-examples' }
    ],

    sidebar: generateSidebar('site'), // Automatically generates sidebar from 'docs'
    // sidebar: [
    //   {
    //     text: 'Examples',
    //     items: [
    //       { text: 'Markdown Examples', link: '/markdown-examples' },
    //       { text: 'Runtime API Examples', link: '/api-examples' }
    //     ]
    //   }
    // ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/abr-designs/jam-starter-package/' }
    ],

    search: {
      provider: 'local'
    }
  }
})
