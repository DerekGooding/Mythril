// --- State ---
let currentView = 'standard';
const viewport = document.getElementById('viewport');
const svg = document.getElementById('graph-svg');
const tooltip = document.getElementById('tooltip');
const sidebar = document.getElementById('sidebar');

let nodes = [];
let edges = [];
let allEdges = [];
let transform = { x: 50, y: 50, k: 0.6 };
let nodeMap = new Map();

let showSimOverlay = false;
