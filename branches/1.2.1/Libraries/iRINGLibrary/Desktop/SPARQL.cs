using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.library
{
    public class SPARQL
    {
        private List<SPARQLUri> prefixes;
        private List<Clause> clauses;

        public SPARQL()
        {
            prefixes = new List<SPARQLUri>();
            clauses = new List<Clause>();

            prefixes.Add(new SPARQLUri("eg",    @"http://example.org/data"));
            prefixes.Add(new SPARQLUri("rdl",   @"http://rdl.rdlfacade.org/data"));
            prefixes.Add(new SPARQLUri("rdf",   @"http://www.w3.org/1999/02/22-rdf-syntax-ns"));
            prefixes.Add(new SPARQLUri("rdfs",  @"http://www.w3.org/2000/01/rdf-schema"));
            prefixes.Add(new SPARQLUri("xsd",   @"http://www.w3.org/2001/XMLSchema"));
            prefixes.Add(new SPARQLUri("dm",    @"http://dm.rdlfacade.org/data"));
            prefixes.Add(new SPARQLUri("tpl",   @"http://tpl.rdlfacade.org/data"));
        }

        public void AddPrefix(SPARQLUri prefix)
        {
            prefixes.Add(prefix);
        }

        public void AddClause(Clause clause)
        {
            clauses.Add(clause);
        }

        public string Serialize()
        {
            return string.Empty;
        }
    }

    public class SPARQLUri
    {
        public Uri baseUri;
        public string prefix;

        public SPARQLUri(string prefix, string baseUri)
        {
            this.baseUri = new Uri(baseUri);
            this.prefix = prefix;
        }

        public SPARQLUri(string prefix, Uri baseUri)
        {
            this.baseUri = baseUri;
            this.prefix = prefix;
        }
    }

    class Triple
    {
        bool isOptional;
        List<Tuple> nestedTriples;
        Subject subject;
        public Predicate predicate;
        public Objct objct;

        public Triple(Subject subject, Predicate predicate, Objct objct)
        {
            isOptional = false;
            nestedTriples = new List<Tuple>();
            this.subject = subject;
            this.predicate = predicate;
            this.objct = objct;
        }

        public void AddNestedTriple(Predicate predicate, Objct objct)
        {
            nestedTriples.Add(new Tuple(predicate, objct));
        }
    }

    class Tuple
    {
        //look into inheritance
        public Predicate predicate;
        public Objct objct;

        public Tuple(Predicate predicate, Objct objct)
        {
            this.predicate = predicate;
            this.objct = objct;
        }
    }

    class Subject
    {
        string subject;
        string blankNode = string.Empty;

        public Subject(SPARQLUri uri, string ID)
        {
            subject = String.Format("<{0}#{1}>", uri.prefix, ID);
        }

        public Subject()
        {
            subject = String.Format("_:{0}", blankNode);
        }
    }

    class Predicate
    {
        Uri uri;

        public Predicate(Uri uri)
        {
            this.uri = uri;
        }
    }

    class Objct
    {
        string literal;
        Uri uri;
        ObjctType type;

        public Objct(string literal)
        {
            this.literal = literal;
            type = ObjctType.Literal;
        }

        public Objct(Uri uri)
        {
            this.uri = uri;
            type = ObjctType.Uri;
        }

        public enum ObjctType
        {
            Literal,
            Uri
        }
    }

    class Select : Clause
    {
        bool isDistinct { set; get; }
        private List<Variable> variables;

        public Select()
        {
            isDistinct = false;
            variables = new List<Variable>();
            type = SPARQLType.Select;
        }

        public void AddVariable(Variable variable)
        {
            variables.Add(variable);
        }
    }

    class Delete : Clause
    {
        bool isDistinct;
        private List<Variable> variables;

        public Delete()
        {
            isDistinct = false;
            variables = new List<Variable>();
            type = SPARQLType.Delete;
        }

        public void AddVariable(Variable variable)
        {
            variables.Add(variable);
        }
    }

    class Where : Clause
    {
        private List<Triple> triples;

        public Where()
        {
            triples = new List<Triple>();
        }

        public void AddTriple(Triple triple)
        {
            triples.Add(triple);
        }
    }

    public class Clause
    {
        public SPARQLType type;

        public Clause()
        {

        }

        public void SetSPARQLType(SPARQLType type)
        {
            this.type = type;
        }

        public enum SPARQLType
    {
        Select,
        Delete,
        Modify
    }
    }

    class Constraint
    {
        public enum ConstraintType
        {
            Optional,
            Filter
        }
    }

    class Insert
    {
        
    }

    class Variable
    {
        string variable;
        string name;

        public Variable()
        {
            variable = String.Empty;
        }

        public void SetVariable(string variable)
        {
            name = variable;
            this.variable = String.Format("?", variable);
        }
    }
}