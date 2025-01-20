import { resolve } from 'node:path';
import { readdirSync, statSync } from 'node:fs';

// Custom ignore filter list
// TODO -- move this into a parameter passed into function
const excludeList = ["home.md"];

/**
 * Generates a sidebar configuration for VitePress.
 * @param {string} dirPath - The base directory containing the docs.
 * @returns {object[]} - Sidebar configuration.
 */
export function generateSidebar(dirPath) {
  const rootPath = resolve(process.cwd(), dirPath);

  function scanDir(path, base = '') {

    let entries = readdirSync(path)
      .filter(file => !file.startsWith('.') && !file.startsWith('index') && !excludeList.includes(file)) // Ignore hidden files and `index.md`
      .map(file => ({
        name: file,
        path: resolve(path, file),
        isDir: statSync(resolve(path, file)).isDirectory()
      }));

    // Ignore directories that contain no md files
    if (entries.filter(e => e.name.includes(".md")).length === 0) return [];

    // Order plain files on top first
    let fileEntries = [];
    let dirEntries = [];
    entries.forEach(e => {
      if(e.name.includes(".")) fileEntries.push(e);
      else dirEntries.push(e);
    })
    entries = fileEntries.concat(dirEntries);

    const result = entries.map(entry => {
      const relativePath = `${base}/${entry.name}`.replace(/\.md$/, '');

      if (entry.isDir) {
        let items = scanDir(entry.path, relativePath) // Recursively scan subdirectories
        if (items.length > 0) {
          return {
            text: capitalize(entry.name),
            collapsible: true,
            items: scanDir(entry
              .path, relativePath) // Recursively scan subdirectories
          };
        } else {
          return {}
        }
      }

      return { text: capitalize(entry.name.replace(/\.md$/, '')), link: relativePath };
    });

    return result;
  }

  return scanDir(rootPath);
}

/**
 * Capitalizes the first letter of a string.
 * @param {string} str - The string to capitalize.
 * @returns {string} - The capitalized string.
 */
function capitalize(str) {
  return str.charAt(0).toUpperCase() + str.slice(1);
}
