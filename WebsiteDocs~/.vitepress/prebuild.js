// Update any links etc from docs -- change .cs links to open github
const fs = require("node:fs/promises")
const path = require("node:path")
const { glob } = require('glob');

const repoUrl = 'https://github.com/abr-designs/jam-starter-package/blob/main/';
const docsDir = path.join(__dirname, "../site");

function collapseSlashes(url) {
    
    return url.replace(/(?<!https:)\/{2,}/g, '/');
}

// Function to replace .cs file links with GitHub URLs
const fixCsLinks = (content) => {
  return content.replace(
    /\[(.*?)\]\(([^)\s]+\.cs)\)/gi,
    (_match, text, filePath) => {
        const cleanPath = filePath
            .replace(/^(\.\.\/)+/, '')
            .replace(/^\.\//, '');

      return `[${text}](${repoUrl}${cleanPath})`;
    }
  );
};

// Function to replace links starting with 'Documentation~' to be relative to the root
const fixDocLinks = (content) => {
    // Replace 'Documentation~' with the correct URL for documentation links
    return content.replace(
        /\[(.*?)\]\(\/?Documentation(?:~|%7E)\/([^)]+)\)/gi,
        (_match, text, relativePath) => {
            return `[${text}](/${relativePath})`;
        }
    );
  };

// Ensure image links are absolute to asset path
const fixImageAssets = (content) => {
    // Replace all instances of ../Images with /Images
    return content.replace(
        /\((?:\/|\.\.\/|\.\/)?Images\/([^)\s]+)\)/gi,
        (_match, imagePath) => {
            const extension = path.extname(imagePath);

            const normalizedPath = extension
                ? imagePath.slice(0, -extension.length) +
                extension.toLowerCase()
                : imagePath;

            return `(/Images/${normalizedPath})`;
        }
    );
}

// Replace checkbox with emoji (- [])
const replaceCheckboxes = (content) => {
 let newContent = content.replace(/- \[ \]/g, (match) => {
    return '- :white_large_square:';
  });
  return newContent.replace(/- \[[x,X]\]/g, (match) => {
   return '- :white_check_mark:'; 
  });
}

// Function to process markdown files
const processMarkdownFiles = async () => {
    const mdFiles = await glob(`${docsDir}/**/*.md`, {nodir: true});

    for (const file of mdFiles) {
        
        // Read the file, process the content, and write it back
        let content = await fs.readFile(file, 'utf-8');

        let updatedContent = fixCsLinks(content);
        updatedContent = fixDocLinks(updatedContent);
        updatedContent = fixImageAssets(updatedContent);
        updatedContent = replaceCheckboxes(updatedContent);

        if (updatedContent !== content) {
            console.log(`Updated links in ${file}`);
            await fs.writeFile(file, updatedContent, 'utf-8');
        }
    }
}


// Ensure all extensions are lowercase
const processExtensions = async () => {
    const files = await glob(`${docsDir}/**/*.*`, {nodir: true});

    for (const file of files) {
        const ext = path.extname(file);
        if(ext === ext.toUpperCase())
        {  
            const oldPath = file;
            const newPath = file.replace(ext, ext.toLocaleLowerCase());
            await fs.rename(oldPath, newPath);
            console.log(`Renamed ${oldPath} to ${newPath}`);
        }
    }
}

async function main() {
    try {

        console.log('Starting markdown link processing...');
        await processMarkdownFiles();
        console.log('Markdown file links - Prebuild step completed!');

        console.log('Starting image extension processing...');
        await processExtensions();
        console.log('Image rename - Prebuild step completed!');

    } catch (err) {
        console.error('Error during prebuild:', err);
        process.exitCode = 1;
    }
}

main();