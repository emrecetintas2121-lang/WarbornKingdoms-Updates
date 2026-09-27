import {readFileSync} from 'node:fs';
import {runInNewContext} from 'node:vm';
const root=new URL('../website/',import.meta.url);
const html=readFileSync(new URL('index.html',root),'utf8');
const script=readFileSync(new URL('site.js',root),'utf8');
const texts=[...html.matchAll(/data-i18n="([^"]+)"/g)].map(m=>({dataset:{i18n:m[1]},textContent:''}));
const buttons=['en','tr','ru'].map(language=>({dataset:{language},classList:{toggle(){}},setAttribute(){},addEventListener(_name,fn){this.click=fn}}));
const document={documentElement:{lang:''},querySelectorAll(selector){return selector==='[data-i18n]'?texts:buttons}};
runInNewContext(script,{document,localStorage:{getItem(){return null},setItem(){}}});
if(document.documentElement.lang!=='en')throw new Error('Default must be English');
let checks=0;for(const button of buttons){button.click();if(document.documentElement.lang!==button.dataset.language)throw new Error('Selection failed');for(const el of texts){if(!el.textContent)throw new Error('Missing translation: '+el.dataset.i18n);checks++;}}
if(/europe1100-hero|realm\.png/.test(html+readFileSync(new URL('style.css',root),'utf8')))throw new Error('Old generated art still in use');
console.log(`PASS: ${checks} website translation checks, EN default, language switching and no generated hero references.`);
