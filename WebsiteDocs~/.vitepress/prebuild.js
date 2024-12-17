// Update any links etc from docs -- change .cs links to open github
const fs = require("node:fs/promises")
const path = require("node:path")
const { glob } = require('glob');

const repoUrl = 'https://github.com/abr-designs/jam-starter-package/blob/main/';
const docsDir = path.join(__dirname, "../site");

// Function to replace .cs file links with GitHub URLs
const fixCsLinks = (content) => {
    return content.replace(/\[([^\]]+)\]\(([^)]+\.cs)\)/g, (match, text, filePath) => {
        const absoluteUrl = `${repoUrl}${filePath}`;
        return `[${text}](${absoluteUrl})`;
    });
};
// Function to replace links starting with 'Documentation~' to be relative to the root
const fixDocLinks = (content) => {
    return content.replace(/\[([^\]]+)\]\((Documentation~[^\)]+)\)/g, (match, text, docPath) => {
      // Replace 'Documentation~' with the correct URL for documentation links
      const relativeUrl = `${docPath.replace('Documentation~', '')}`; // Adjust URL
      return `[${text}](${relativeUrl})`;
    });
  };

// Function to replace containing capital extensions (ex .PNG)
const fixCapitalLinks = (content) => {
    // Replace all links with capital extensions to lowercase extensions
    return content.replace(/\(([^)]+?)(\.[A-Z]+)\)/g, (match, url, ext) => {
        const newUrl = `${url}${ext.toLowerCase()}`;
        return `(${newUrl})`;
      });
  };

// Function to process markdown files
const processMarkdownFiles = async () => {
    const files = await fs.readdir(docsDir);

    for (const file of files) {
        const filePath = path.join(docsDir, file);

        if ((await fs.stat(filePath)).isDirectory()) {
            continue;
        }

        // Read the file, process the content, and write it back
        let content = await fs.readFile(filePath, 'utf-8');

        let updatedContent = fixCsLinks(content);
        updatedContent = fixDocLinks(updatedContent);
        updatedContent = fixCapitalLinks(updatedContent);

        if (updatedContent !== content) {
            console.log(`Updated links in ${file}`);
            await fs.writeFile(filePath, updatedContent, 'utf-8');
        }
    }
};


// Ensure all extensions are lowercase
const processExtensions = async () => {
    const files = await glob(`${docsDir}/**/*.*`, {nodir: true});

    files.forEach(async file => {
        const ext = path.extname(file);
        if(ext === ext.toUpperCase())
        {  
            const oldPath = file;
            const newPath = file.replace(ext, ext.toLocaleLowerCase());
            await fs.rename(oldPath, newPath);
            console.log(`Renamed ${oldPath} to ${newPath}`);
        }
    })
}

processMarkdownFiles()
    .then(() => console.log('Markdown file links - Prebuild step completed!'))
    .catch((err) => console.error('Error during prebuild:', err));

processExtensions()
    .then(() => console.log('Image rename - Prebuild step completed!'))
    .catch((err) => console.error('Error during prebuild:', err));
