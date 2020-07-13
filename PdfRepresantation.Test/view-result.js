var urls = [];
var currentIndex = 0;


function setHeight(id) {
    var articles = getFrameDoc(id).getElementsByClassName('article');
    var article = articles[articles.length - 1];
    var heightStr = article.style.height;
    heightStr = heightStr.substr(0, heightStr.length - 2);
    var top = (+heightStr + 30) * articles.length + 100;
    document.getElementById(id).height = top + 'px';

}

function initFrame(id) {

    changeZoomOfFrame(id)
    setHeight(id);
    // setTimeout(function () {
    // var frame=getFrameDoc(id);
    // var bodyWidth=document.body.offsetWidth;
    // var frameWidthStr=frame.getElementsByClassName('article')[0].style.width;
    // frameWidthStr = +frameWidthStr.substr(0, frameWidthStr.length - 2)+16;
    // //frame.body.style.margin = 0;
    // frame.body.style.zoom =(bodyWidth/+frameWidthStr)*100+ "%" ;
    // },20);
}

function getFrameDoc(id) {
    var frame = document.getElementById(id);
    var innerDoc = frame.contentWindow || frame.contentDocument;
    return innerDoc.document;

}
function moveBy(i) {
    if (!i )
        return;
    currentIndex += i;
    indexChanged()
}
function indexChanged() {
    if (currentIndex < 0)
        currentIndex = 0;
    if (currentIndex >= urls.length)
        currentIndex = urls.length - 1;
    var url = urls[currentIndex];
    // var current=window.location.href;
    // var curIndex=current.indexOf("Mail");
    // if (curIndex>=0)
    // {
    // var startCurrent=current.substr(0,curIndex);
    // url = startCurrent+url.substr(url.indexOf("Mail"));
    // }
    try {

        document.getElementById('frames').scroll(0, 0);
    }
    catch (e) {

    }
    document.getElementById('url').innerText = url.substr(url.lastIndexOf("\\") + 1);
    document.getElementById('text').value = currentIndex;
    document.getElementById('frame').src = url;
}

function init() {
    var input = document.getElementById("text");
    input.addEventListener("keyup", function (event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            moveBy(+(document.getElementById("text").value) - currentIndex);
        }
    });
    indexChanged();
}

function changeZoom() {
    changeZoomOfFrame('frame')
}

var setValue = function (item, prop, zoom) {
    if (item[prop + '-origin']) {
        item.style[prop] = (item[prop + '-origin'] * zoom) + '';
        return
    }
    if (item[prop + '-origin-px']) {
        item.style[prop] = (item[prop + '-origin-px'] * zoom) + 'px';
        return
    }
    var result = item.style[prop];
    if (!result)
        return;
    if (result.constructor === String && result.endsWith('px')) {
        result = result.substr(0, result.length - 2);
        item[prop + '-origin-px'] = result;
        item.style[prop] = ((+result) * zoom) + 'px';
    }
    else {
        item[prop + '-origin'] = result;
        item.style[prop] = ((+result) * zoom) + '';
    }

};
var setAttribute = function (item, prop, zoom) {
    if (item[prop + '-origin']) {
        item.setAttribute(prop, (item[prop + '-origin'] * zoom) + '');
        return
    }
    if (item[prop + '-origin-px']) {
        item.setAttribute(prop, (item[prop + '-origin-px'] * zoom) + 'px');
        return
    }
    var result = item.getAttribute(prop);
    if (!result)
        return;
    if (result.constructor === String && result.endsWith('px')) {
        result = result.substr(0, result.length - 2);
        item[prop + '-origin-px'] = result;
        item.setAttribute(prop, ((+result) * zoom) + 'px');
    }
    else {
        item[prop + '-origin'] = result;
        item.setAttribute(prop, ((+result) * zoom) + '');
    }

};
var setPath = function (path, zoom) {
    var origin = path['origin'];
    if (!origin) {
        var d=path.getAttribute('d');
        var s='';
        origin=[];
        for (var i = 0; i < d.length; i++) {
            
            switch (d[i])
            {
                case ' ':if (s.length>0){origin.push(s);s = '';}break;
                case ',':if (s.length>0){origin.push(s);s = '';}
                origin.push(',');                 
                break;
                default:s+=d[i]; break;
            }        
        }
        if (s.length>0)            origin.push(s);
        path['origin'] = origin;
    }
    var converted = [];
    for (var i = 0; i < origin.length; i++) {
        var item = origin[i];
        if (!isNaN(item))
            item = (+item) * zoom;
        converted.push(item);
    }

    path.setAttribute('d', converted.join(' ').replace(' ,',','));


};
var setFontSize = function (rule, zoom) {
    if (!rule.selectorText.startsWith(".font-size-"))
        return;
    var origin = rule['origin'];
    if (!origin) {
        origin = rule.style.fontSize;
        origin = origin.substr(0, origin.length - 2);
        origin = +origin;
        rule['origin'] = origin;
    }
    rule.style.fontSize = (origin * zoom) + 'px';
}

function changeZoomOfFrame(frameId) {
    var zoom = (+document.getElementById('zoom').value) / 100.0;

    var doc = getFrameDoc(frameId);
    var items = doc.getElementsByClassName('article');
    for (var i = 0; i < items.length; i++) {
        setValue(items[i], 'width', zoom);
        setValue(items[i], 'height', zoom);
        setValue(items[i], 'margin-top', zoom);
    }
    items = doc.getElementsByClassName('canvas');
    for (var i = 0; i < items.length; i++) {
        setValue(items[i], 'width', zoom);
        setAttribute(items[i], 'width', zoom);
        setValue(items[i], 'height', zoom);
        setAttribute(items[i], 'height', zoom);
    }
    items = doc.getElementsByClassName('header');
    for (var i = 0; i < items.length; i++) {
        setValue(items[i], 'width', zoom);
    }
    items = doc.getElementsByClassName('image');
    for (var i = 0; i < items.length; i++) {
        setAttribute(items[i], 'width', zoom);
        setAttribute(items[i], 'height', zoom);
        setValue(items[i], 'right', zoom);
        setValue(items[i], 'left', zoom);
        setValue(items[i], 'top', zoom);
    }
    items = doc.getElementsByTagName('image');
    for (var i = 0; i < items.length; i++) {
        setAttribute(items[i], 'width', zoom);
        setAttribute(items[i], 'height', zoom);
        setAttribute(items[i], 'x', zoom);
        setAttribute(items[i], 'y', zoom);
    }
    items = doc.getElementsByClassName('line');
    for (var i = 0; i < items.length; i++) {
        setValue(items[i], 'right', zoom);
        setValue(items[i], 'left', zoom);
        setValue(items[i], 'top', zoom);
        setValue(items[i], 'bottom', zoom);
        setValue(items[i], 'width', zoom);
        setValue(items[i], 'height', zoom);
    }
    items = doc.getElementsByTagName('path');
    for (var i = 0; i < items.length; i++) {
        setPath(items[i], zoom);
        setAttribute(items[i], 'stroke-width', zoom);
    }
    var sheets = doc.styleSheets;
    if (sheets.length !== 0) {
        var sheet = sheets[sheets.length - 1];
        var rules = sheet.cssRules || sheet.rules;
        for (var i = 0; i < rules.length; i++) {
            setFontSize(rules[i], zoom);
        }
    }
}
