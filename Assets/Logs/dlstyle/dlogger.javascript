/**
	Functions for toggling visibility in the log.
*/


/**
	Gets all the elements in the node of the given  class and tag.
*/
function getElementsByClass(searchClass,node,tag) {
	var classElements = new Array();
	
	if ( node == null )
	{
		node = document;
	}
	
	if ( tag == null )
	{
		tag = '*';
	}
	
	var elements = node.getElementsByTagName(tag);
	var elementsSize = elements.length;
	var pattern = new RegExp("(^|\\s)" + searchClass + "(\\s|$)");
	
	for (i = 0, j = 0; i < elementsSize; i++) 
	{
		if ( pattern.test(elements[i].className) ) 
		{
			classElements[j] = elements[i];
			j++;
		}
	}
	
	return classElements;
}

/**
	For toggling visibility of stacks in the HTML log.
*/
function hide(id) 
{
	var element_style = document.getElementById(id).style;
	var status = element_style.display;
	
	if (status != 'block') 
	{
		element_style.display = 'block';
	}
	else //status == visible
	{
		element_style.display = 'none';
	}
}

/**
	For toggling visibility of a class in the HTML log.
*/
function hide_class(className)
{
	var elements = getElementsByClass(className);
	
	var pattern = new RegExp("(^|\\s)Button(\\s|$)");
	
	for(i = 0; i < elements.length; i++)
	{		
		if(!pattern.test(elements[i].className))
		{
			if(elements[i].style.display != 'none')
			{
				elements[i].style.display = 'none'
			}
			else
			{
				elements[i].style.display = 'block'
			}
		}
	}
}

/**
	For enabling visibility of all classes in the HTML log.
*/
function enable_all(divName)
{
	var element = document.getElementById(divName);
	
	var classList = element.className.split(/\s+/);
	
	for (var i = 0; i < classList.length; i++) 
	{
		if (classList[i] === 'Header' || classList[i] === 'Usage' || classList[i] === 'date' || classList[i] === 'Time' || classList[i] === 'Icon') continue;
		
		var elements = getElementsByClass(classList[i]);
		
		for(j = 0; j < elements.length; j++)
		{
			elements[j].style.display = 'block'
		}
	}
}

/**
	For enabling visibility of all classes in the HTML log.
*/
function disable_all(divName)
{
	var element = document.getElementById(divName);

	var classList = element.className.split(/\s+/);
	
	for (var i = 0; i < classList.length; i++) 
	{
		if (classList[i] === 'Header' || classList[i] === 'Usage' || classList[i] === 'date' || classList[i] === 'Time' || classList[i] === 'Icon') continue;
		
		var elements = getElementsByClass(classList[i]);
		
		for(j = 0; j < elements.length; j++)
		{
			elements[j].style.display = 'none'
		}
	}
}
