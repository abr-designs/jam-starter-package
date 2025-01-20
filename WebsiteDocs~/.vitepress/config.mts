import { defineConfig } from 'vitepress'
import { generateSidebar } from './sidebar-generator'
import MarkdownIt from 'markdown-it'
import Token from 'markdown-it/lib/token.mjs';

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "Jam Starter Package",
  description: "Documentation",
  srcDir: './site', // Specify the location of Markdown files
  base: process.env.VITE_BASE_URL || '/', // needs to be changed for building on github,
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/home' }
      //{ text: 'Examples', link: '/markdown-examples' }
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
  },
  markdown: {
    config: (md) => {
      const proxy = (tokens, idx, options, env, self) => self.renderToken(tokens, idx, options);
      const defaultBulletListOpenRenderer = md.renderer.rules.bullet_list_open || proxy;

      // Extend Markdown-It to add custom logic
      md.renderer.rules.bullet_list_open = (tokens, idx, options, env, self) => {
        // Check all list items within the current <ul>
        const ulTokens = tokens.slice(idx + 1); // Skip the <ul> token itself
        const nextUlCloseIndex = ulTokens.findIndex(t => t.type === 'bullet_list_close');
        const listItems = ulTokens.slice(0, nextUlCloseIndex); // Get only tokens inside this <ul>

        // Check if any list item starts with ✅ or ⬜
        let containsCheckbox = false;
        for (let i = 0; i < listItems.length; i++) {
          const token = listItems[i];
          if (token.type === 'list_item_open') {
            // Collect all content between list_item_open and list_item_close
            const liContentTokens: Token[] = [];
            let j = i + 1;
            while (listItems[j] && listItems[j].type !== 'list_item_close') {
              liContentTokens.push(listItems[j]);
              j++;
            }

            // Extract plain text content from these tokens
            const liTextContent = liContentTokens
              .map(t => t.content || (t.children ? t.children.map(c => c.content).join('') : ''))
              .join('');
            
            if (liTextContent.trim().startsWith(':white_check_mark') || liTextContent.trim().startsWith(':white_large_square:')) {
              containsCheckbox = true;
              break; // No need to check further
            }
          }
        }

        // Add a custom class if an emoji is detected
        if (containsCheckbox) {
          tokens[idx].attrJoin('class', 'docs-checklist');
        }

        // Default rendering
        // return self.renderToken(tokens, idx, options);
        return defaultBulletListOpenRenderer(tokens, idx, options, env, self);
      };
    
    },
  },
})

function getFirstListItemText(tokens: Token[]) : Token | null{

  for(let i=0; i<tokens.length;i++) {
    if(tokens[i].content.length > 0) return tokens[i];
    if(tokens[i].children && tokens[i].children!.length > 0) {
      let childItem = getFirstListItemText(tokens[i].children!);
      if(childItem) return childItem;
    }
  }
  return null;
  
}
