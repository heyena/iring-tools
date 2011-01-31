package org.ids_adi.csv;

import java.io.*;
import javax.xml.transform.*;
import javax.xml.transform.sax.*;
import javax.xml.transform.stream.*;
import org.xml.sax.*;
import org.xml.sax.helpers.*;

/**
 * This version of the parser is very lax and will consume
 * input until file end.  Nothing is invalid.  Quotes
 * within fields delimit quoted sections as per the spec,
 * but you can have multiple quoted sections in one field
 * if you intersperse them with non-quote characters, which
 * is kind of weird.
 *
 */
public class CSVParser {
    private final StringBuffer buffer = new StringBuffer();
    private Reader reader;
    private char line = '\n';
    private char cell = ',';
    private char quote = '"';
    private boolean atLine = true;
    private int c;

    public CSVParser() {
    }

    protected void nextCharacter() throws IOException {
    	if (c >= 0) {
	    c = reader.read();

	    if (c == 13) {
		c = reader.read();
	    }
	}
    }

    public void init(Reader reader) throws IOException {
    	this.reader = reader;

	nextCharacter();
    }

    public boolean hasNextLine() throws IOException {
	if (!atLine) {
	    throw new IllegalStateException();
	}

	return (c >= 0);
    }
    
    public boolean hasNextCell() throws IOException {
	return (!atLine);
    }

    public void nextLine() throws IOException {
    	if (!hasNextLine()) {
	    throw new IllegalStateException();
	}

	atLine = false;
    }

    public String nextCell() throws IOException {
    	if (!hasNextCell()) {
	    throw new IllegalStateException();
	}

	buffer.setLength(0);

	while ((c != line) && (c != cell) && (c >= 0)) {
	    if (c == quote) {
		while (true) {
		    nextCharacter();

		    if (c != quote) {
			buffer.append((char)c);
		    }
		    else {
			nextCharacter();

			if (c == quote) {
			    buffer.append((char)c);
			}
			else {
			    break;
			}
		    }
		}
	    }
	    else {
		buffer.append((char)c);
		nextCharacter();
	    }
	}

	if (c == line) {
	    nextCharacter();
	    atLine = true;
	}
	else if (c == cell) {
	    nextCharacter();
	}
	else {
	    atLine = true;
	}

	return (buffer.toString());
    }

    public static void main(String args[]) throws Throwable {
    	final CSVParser parser = new CSVParser();
	final Reader reader = new InputStreamReader(
	    new FileInputStream(args[0]), "UTF-8");
	final TransformerHandler handler = 
	    ((SAXTransformerFactory)TransformerFactory.newInstance()).
	    newTransformerHandler();
	final Attributes emptyAttributes = new AttributesImpl();
	final char lineEnd[] = "\n".toCharArray();
	final char indent[] = "    ".toCharArray();

	handler.setResult(new StreamResult(System.out));

	handler.startDocument();

	try {
	    parser.init(reader);

	    handler.startElement("", "csv", "csv", emptyAttributes);

	    while (parser.hasNextLine()) {
		parser.nextLine();
		handler.ignorableWhitespace(indent, 0, indent.length);
		handler.startElement("", "line", "line", emptyAttributes);

		while (parser.hasNextCell()) {
		    handler.startElement("", "cell", "cell", emptyAttributes);
		    char array[] = parser.nextCell().toCharArray();
		    handler.characters(array, 0, array.length);
		    handler.endElement("", "cell", "cell");
		}

		handler.endElement("", "line", "line");
		handler.ignorableWhitespace(lineEnd, 0, lineEnd.length);
	    }

	    handler.endElement("", "csv", "csv");
	}
	finally {
	    reader.close();
	}

	handler.endDocument();
    }
}

