// --- State ---
let currentView = 'lattice';
const viewport = document.getElementById('viewport');
const svg = document.getElementById('graph-svg');
const tooltip = document.getElementById('tooltip');
const sidebar = document.getElementById('sidebar');

let nodes = [];
let edges = [];
let allEdges = [];
let transform = { x: 50, y: 50, k: 0.6 };
let nodeMap = new Map();
let simulationFrame = 0;
let isSimulating = true;
let draggedNode = null;

let showProgressionOnly = true;
let showHubs = false;
let showSimOverlay = false;
