import { resolve, basename } from 'node:path';
import { readdirSync, statSync, readFileSync } from 'node:fs';
import matter from 'gray-matter';

// Custom ignore filter list
// TODO -- move this into a parameter passed into function
const excludeList = ["index.md", "home.md"];

/**
 * Generates a sidebar configuration for VitePress.
 * @param {string} dirPath - The base directory containing the docs.
 * @returns {object[]} - Sidebar configuration.
 */
export function generateSidebar(dirPath) {
  const rootPath = resolve(process.cwd(), dirPath);

  // Get a list of files/folders in a path
  function getFolderEntries(path) {
    let entries = readdirSync(path)
      .filter(file => !file.startsWith('.')) // Ignore hidden files and `index.md`
      .map(file => ({
        name: file,
        path: resolve(path, file),
        isDir: statSync(resolve(path, file)).isDirectory()
      }));
    return entries;
  }

  // Recursively check if this directory has markdown files
  function dirHasMarkdown(path) {
    const entries = getFolderEntries(path);
    for(const e of entries) {
      
      if(e.isDir && dirHasMarkdown(e.path)) {
        return true;
      }

      if(e.name.toLowerCase().endsWith('.md')) {
        return true;
      }

    }
    return false;
  }

  function scanDir(path, base = '') {

    let entries = getFolderEntries(path);

    let fileEntries = entries.filter(e => !e.isDir);
    let dirEntries = entries.filter(e => e.isDir && dirHasMarkdown(e.path));

    const fileItems = []
    const dirItems = []

    // process files
    for (const entry of fileEntries) {

      if(excludeList.includes(entry.name.toLowerCase())) {        
        continue;
      }

      const relativePath = `${base}/${entry.name}`.replace(/\.md$/, '');
      fileItems.push(
        { text: getPageTitle(entry.path), link: relativePath }
      )
    }

    // process directories
    for (const entry of dirEntries) {
      const relativePath = `${base}/${entry.name}`.replace(/\.md$/, '');

      let items = scanDir(entry.path, relativePath) // Recursively scan subdirectories
      if (items.length === 0) return; // skip empty directory

      // TODO -- if a directory has a index.md item -- this should also be hoisted
      // Only one file entry -- we will hoist this folder to a file item
      if (items.length === 1 && !items[0].items) {
        fileItems.push(items[0]);
        continue;
      }

      // Otherwise we build a folder
      dirItems.push({
        text: capitalize(entry.name),
        collapsible: true,
        items: items
      })

    }

    fileItems.sort((a, b) => { a.text.localeCompare(b.text) });
    dirItems.sort((a, b) => { a.text.localeCompare(b.text) });
    const result = fileItems.concat(dirItems);
    return result;

  }

  const result = scanDir(rootPath);
  return result;
}

/**
 * Capitalizes the first letter of a string.
 * @param {string} str - The string to capitalize.
 * @returns {string} - The capitalized string.
 */
function capitalize(str) {
  return str.charAt(0).toUpperCase() + str.slice(1);
}

/**
 * Reads the frontmatter of .md files to extract title
 */
function getPageTitle(filePath) {

  let title = '';
  try {
    const content = readFileSync(filePath);

    // extract YAML data
    const { data } = matter(content);
    title = data.title;

  } catch (err) {
    console.error('error reading page YAML data', err);
  }

  // Return the title or the filename as default
  return title || capitalize(basename(filePath).replace(/\.md$/, ''));

}